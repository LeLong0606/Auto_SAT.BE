using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using SAT.BE.src.SAT.BE.Application.Common;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;
using SAT.BE.src.SAT.BE.Api.Authorization;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;

namespace SAT.BE.src.SAT.BE.Api.Controllers.Scheduling
{
    /// <summary>
    /// Shift Assignment management controller with role-based authorization for creating and managing schedules
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ShiftAssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShiftAssignmentsController> _logger;

        public ShiftAssignmentsController(
            ApplicationDbContext context,
            ILogger<ShiftAssignmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get shift assignments with pagination - Team leaders can view their team, Directors can view all
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="employeeId">Filter by employee ID (optional)</param>
        /// <param name="shiftId">Filter by shift ID (optional)</param>
        /// <param name="date">Filter by date (optional)</param>
        /// <returns>Paginated list of shift assignments</returns>
        [HttpGet]
        [HasPermission(PermissionConstants.SCHEDULE_VIEW)]
        public async Task<ActionResult<ServiceResult<PagedResult<object>>>> GetShiftAssignments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? employeeId = null,
            [FromQuery] int? shiftId = null,
            [FromQuery] DateTime? date = null)
        {
            try
            {
                _logger.LogInformation("Getting shift assignments - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.ShiftAssignments
                    .Include(sa => sa.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(sa => sa.Shift)
                    .AsQueryable();

                // Apply role-based filtering
                query = ApplyRoleBasedFiltering(query);

                // Apply additional filters
                if (employeeId.HasValue)
                {
                    query = query.Where(sa => sa.EmployeeId == employeeId.Value);
                }

                if (shiftId.HasValue)
                {
                    query = query.Where(sa => sa.ShiftId == shiftId.Value);
                }

                if (date.HasValue)
                {
                    query = query.Where(sa => sa.Date.Date == date.Value.Date);
                }

                var totalCount = await query.CountAsync();
                var assignments = await query
                    .OrderByDescending(sa => sa.Date)
                    .ThenBy(sa => sa.Employee.FullName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(sa => new
                    {
                        sa.ShiftAssignmentId,
                        sa.Date,
                        sa.Status,
                        sa.StatusCode,
                        sa.Notes,
                        sa.CheckInTime,
                        sa.CheckOutTime,
                        Employee = new
                        {
                            sa.Employee.EmployeeId,
                            sa.Employee.EmployeeCode,
                            sa.Employee.FullName,
                            sa.Employee.Email,
                            Department = new
                            {
                                sa.Employee.Department.DepartmentId,
                                sa.Employee.Department.DepartmentName
                            }
                        },
                        Shift = new
                        {
                            sa.Shift.ShiftId,
                            sa.Shift.ShiftName,
                            sa.Shift.Type,
                            sa.Shift.StartTime,
                            sa.Shift.EndTime
                        }
                    })
                    .ToListAsync();

                var result = new PagedResult<object>(assignments.Cast<object>().ToList(), totalCount, pageNumber, pageSize);
                return Ok(ServiceResult<PagedResult<object>>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting shift assignments");
                return StatusCode(500, ServiceResult<PagedResult<object>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get shift assignments for a specific employee and date range
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of shift assignments for the employee in the date range</returns>
        [HttpGet("employee/{employeeId:int}")]
        [HasPermission(PermissionConstants.SCHEDULE_VIEW)]
        public async Task<ActionResult<ServiceResult<List<object>>>> GetEmployeeShiftAssignments(
            int employeeId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting shift assignments for employee: {EmployeeId}", employeeId);

                // Check if user can access this employee's data
                if (!await CanAccessEmployeeSchedule(employeeId))
                {
                    return Forbid();
                }

                var query = _context.ShiftAssignments
                    .Where(sa => sa.EmployeeId == employeeId)
                    .Include(sa => sa.Shift)
                    .AsQueryable();

                // Apply date filters
                if (startDate.HasValue)
                {
                    query = query.Where(sa => sa.Date >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(sa => sa.Date <= endDate.Value.Date);
                }

                var assignments = await query
                    .OrderBy(sa => sa.Date)
                    .Select(sa => new
                    {
                        sa.ShiftAssignmentId,
                        sa.Date,
                        sa.Status,
                        sa.StatusCode,
                        sa.Notes,
                        sa.CheckInTime,
                        sa.CheckOutTime,
                        Shift = new
                        {
                            sa.Shift.ShiftId,
                            sa.Shift.ShiftName,
                            sa.Shift.Type,
                            sa.Shift.StartTime,
                            sa.Shift.EndTime
                        }
                    })
                    .ToListAsync();

                return Ok(ServiceResult<List<object>>.Success(assignments.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting shift assignments for employee: {EmployeeId}", employeeId);
                return StatusCode(500, ServiceResult<List<object>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Create shift assignment - Team leaders can create for their team, Directors can create for all
        /// </summary>
        /// <param name="request">Shift assignment creation request</param>
        /// <returns>Created shift assignment</returns>
        [HttpPost]
        [HasPermission(PermissionConstants.SCHEDULE_CREATE)]
        public async Task<ActionResult<ServiceResult<object>>> CreateShiftAssignment([FromBody] CreateShiftAssignmentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating shift assignment for employee: {EmployeeId} on date: {Date}", 
                    request.EmployeeId, request.Date);

                // Check if user can create schedules for this employee
                if (!await CanCreateScheduleForEmployee(request.EmployeeId))
                {
                    return Forbid();
                }

                // Check if assignment already exists for this employee on this date
                var existingAssignment = await _context.ShiftAssignments
                    .FirstOrDefaultAsync(sa => sa.EmployeeId == request.EmployeeId && sa.Date.Date == request.Date.Date);

                if (existingAssignment != null)
                {
                    return BadRequest(ServiceResult<object>.Failure("Shift assignment already exists for this employee on this date.", 400));
                }

                // Validate employee exists
                var employee = await _context.Employees.FindAsync(request.EmployeeId);
                if (employee == null)
                {
                    return BadRequest(ServiceResult<object>.Failure("Employee not found.", 400));
                }

                // Validate shift exists
                var shift = await _context.Shifts.FindAsync(request.ShiftId);
                if (shift == null)
                {
                    return BadRequest(ServiceResult<object>.Failure("Shift not found.", 400));
                }

                var shiftAssignment = new ShiftAssignment
                {
                    EmployeeId = request.EmployeeId,
                    ShiftId = request.ShiftId,
                    Date = request.Date.Date,
                    Status = ShiftStatus.Scheduled,
                    StatusCode = "S",
                    Notes = request.Notes,
                    CreatedDate = DateTime.UtcNow
                };

                _context.ShiftAssignments.Add(shiftAssignment);
                await _context.SaveChangesAsync();

                // Load related data for response
                await _context.Entry(shiftAssignment)
                    .Reference(sa => sa.Employee)
                    .LoadAsync();
                await _context.Entry(shiftAssignment)
                    .Reference(sa => sa.Shift)
                    .LoadAsync();

                var response = new
                {
                    shiftAssignment.ShiftAssignmentId,
                    shiftAssignment.Date,
                    shiftAssignment.Status,
                    shiftAssignment.StatusCode,
                    shiftAssignment.Notes,
                    Employee = new
                    {
                        shiftAssignment.Employee.EmployeeId,
                        shiftAssignment.Employee.EmployeeCode,
                        shiftAssignment.Employee.FullName
                    },
                    Shift = new
                    {
                        shiftAssignment.Shift.ShiftId,
                        shiftAssignment.Shift.ShiftName,
                        shiftAssignment.Shift.Type,
                        shiftAssignment.Shift.StartTime,
                        shiftAssignment.Shift.EndTime
                    }
                };

                _logger.LogInformation("Shift assignment created successfully with ID: {ShiftAssignmentId}", shiftAssignment.ShiftAssignmentId);
                return CreatedAtAction(nameof(GetShiftAssignment), new { id = shiftAssignment.ShiftAssignmentId },
                    ServiceResult<object>.Success(response, "Shift assignment created successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating shift assignment");
                return StatusCode(500, ServiceResult<object>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get shift assignment by ID
        /// </summary>
        /// <param name="id">Shift Assignment ID</param>
        /// <returns>Shift assignment details</returns>
        [HttpGet("{id:int}")]
        [HasPermission(PermissionConstants.SCHEDULE_VIEW)]
        public async Task<ActionResult<ServiceResult<object>>> GetShiftAssignment(int id)
        {
            try
            {
                _logger.LogInformation("Getting shift assignment with ID: {ShiftAssignmentId}", id);

                var assignment = await _context.ShiftAssignments
                    .Include(sa => sa.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(sa => sa.Shift)
                    .Where(sa => sa.ShiftAssignmentId == id)
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    _logger.LogWarning("Shift assignment with ID {ShiftAssignmentId} not found", id);
                    return NotFound(ServiceResult<object>.Failure("Shift assignment not found.", 404));
                }

                // Check access rights
                if (!await CanAccessEmployeeSchedule(assignment.EmployeeId))
                {
                    return Forbid();
                }

                var result = new
                {
                    assignment.ShiftAssignmentId,
                    assignment.Date,
                    assignment.Status,
                    assignment.StatusCode,
                    assignment.Notes,
                    assignment.CheckInTime,
                    assignment.CheckOutTime,
                    assignment.CreatedDate,
                    assignment.ModifiedDate,
                    Employee = new
                    {
                        assignment.Employee.EmployeeId,
                        assignment.Employee.EmployeeCode,
                        assignment.Employee.FullName,
                        assignment.Employee.Email,
                        Department = new
                        {
                            assignment.Employee.Department.DepartmentId,
                            assignment.Employee.Department.DepartmentName
                        }
                    },
                    Shift = new
                    {
                        assignment.Shift.ShiftId,
                        assignment.Shift.ShiftName,
                        assignment.Shift.Type,
                        assignment.Shift.StartTime,
                        assignment.Shift.EndTime
                    }
                };

                return Ok(ServiceResult<object>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting shift assignment with ID: {ShiftAssignmentId}", id);
                return StatusCode(500, ServiceResult<object>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get today's shift assignments
        /// </summary>
        /// <returns>List of today's shift assignments</returns>
        [HttpGet("today")]
        [HasPermission(PermissionConstants.SCHEDULE_VIEW)]
        public async Task<ActionResult<ServiceResult<List<object>>>> GetTodayShiftAssignments()
        {
            try
            {
                _logger.LogInformation("Getting today's shift assignments");

                var today = DateTime.UtcNow.Date;
                var query = _context.ShiftAssignments
                    .Where(sa => sa.Date.Date == today)
                    .Include(sa => sa.Employee)
                        .ThenInclude(e => e.Department)
                    .Include(sa => sa.Shift)
                    .AsQueryable();

                // Apply role-based filtering
                query = ApplyRoleBasedFiltering(query);

                var assignments = await query
                    .OrderBy(sa => sa.Shift.StartTime)
                    .ThenBy(sa => sa.Employee.FullName)
                    .Select(sa => new
                    {
                        sa.ShiftAssignmentId,
                        sa.Date,
                        sa.Status,
                        sa.StatusCode,
                        sa.CheckInTime,
                        sa.CheckOutTime,
                        Employee = new
                        {
                            sa.Employee.EmployeeId,
                            sa.Employee.EmployeeCode,
                            sa.Employee.FullName,
                            Department = new
                            {
                                sa.Employee.Department.DepartmentId,
                                sa.Employee.Department.DepartmentName
                            }
                        },
                        Shift = new
                        {
                            sa.Shift.ShiftId,
                            sa.Shift.ShiftName,
                            sa.Shift.Type,
                            sa.Shift.StartTime,
                            sa.Shift.EndTime
                        }
                    })
                    .ToListAsync();

                return Ok(ServiceResult<List<object>>.Success(assignments.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting today's shift assignments");
                return StatusCode(500, ServiceResult<List<object>>.Failure("An internal server error occurred.", 500));
            }
        }

        #region Private Helper Methods

        private IQueryable<ShiftAssignment> ApplyRoleBasedFiltering(IQueryable<ShiftAssignment> query)
        {
            // Super admin, admin, director can view all
            if (User.IsInRole(RoleHierarchy.SUPER_ADMIN) || 
                User.IsInRole(RoleHierarchy.ADMIN) || 
                User.IsInRole(RoleHierarchy.DIRECTOR))
            {
                return query;
            }

            // HR can view all
            if (User.IsInRole(RoleHierarchy.HR))
            {
                return query;
            }

            // Team leaders and managers can view their department
            if (User.IsInRole(RoleHierarchy.TEAM_LEADER) || User.IsInRole(RoleHierarchy.MANAGER))
            {
                var departmentIdClaim = User.FindFirst("DepartmentId")?.Value;
                if (int.TryParse(departmentIdClaim, out int departmentId))
                {
                    query = query.Where(sa => sa.Employee.DepartmentId == departmentId);
                }
            }
            else
            {
                // Regular employees can only view their own assignments
                var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
                if (int.TryParse(employeeIdClaim, out int employeeId))
                {
                    query = query.Where(sa => sa.EmployeeId == employeeId);
                }
                else
                {
                    // No access if not an employee
                    query = query.Where(sa => false);
                }
            }

            return query;
        }

        private async Task<bool> CanAccessEmployeeSchedule(int employeeId)
        {
            // Super admin, admin, director can access all
            if (User.IsInRole(RoleHierarchy.SUPER_ADMIN) || 
                User.IsInRole(RoleHierarchy.ADMIN) || 
                User.IsInRole(RoleHierarchy.DIRECTOR))
            {
                return true;
            }

            // HR can access all
            if (User.IsInRole(RoleHierarchy.HR))
            {
                return true;
            }

            // Team leaders and managers can access their department employees
            if (User.IsInRole(RoleHierarchy.TEAM_LEADER) || User.IsInRole(RoleHierarchy.MANAGER))
            {
                var userDepartmentIdClaim = User.FindFirst("DepartmentId")?.Value;
                if (int.TryParse(userDepartmentIdClaim, out int userDepartmentId))
                {
                    var employee = await _context.Employees.FindAsync(employeeId);
                    return employee?.DepartmentId == userDepartmentId;
                }
            }

            // Employees can access their own schedule
            var userEmployeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(userEmployeeIdClaim, out int userEmployeeId))
            {
                return userEmployeeId == employeeId;
            }

            return false;
        }

        private async Task<bool> CanCreateScheduleForEmployee(int employeeId)
        {
            // Super admin, admin, director can create schedules for all employees
            if (User.IsInRole(RoleHierarchy.SUPER_ADMIN) || 
                User.IsInRole(RoleHierarchy.ADMIN) || 
                User.IsInRole(RoleHierarchy.DIRECTOR))
            {
                return true;
            }

            // Team leaders can create schedules for their team members (same department)
            if (User.IsInRole(RoleHierarchy.TEAM_LEADER) || User.IsInRole(RoleHierarchy.MANAGER))
            {
                var userDepartmentIdClaim = User.FindFirst("DepartmentId")?.Value;
                if (int.TryParse(userDepartmentIdClaim, out int userDepartmentId))
                {
                    var employee = await _context.Employees
                        .Include(e => e.WorkPosition)
                        .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                    
                    if (employee?.DepartmentId == userDepartmentId)
                    {
                        // Team leaders can only create schedules for employees below their level
                        if (User.IsInRole(RoleHierarchy.TEAM_LEADER))
                        {
                            var positionLevel = employee.WorkPosition?.Level ?? 1;
                            return positionLevel < 3; // Below team leader level
                        }
                        return true; // Managers can create for anyone in their department
                    }
                }
            }

            return false;
        }

        #endregion
    }

    /// <summary>
    /// Request model for creating shift assignments
    /// </summary>
    public class CreateShiftAssignmentRequest
    {
        public int EmployeeId { get; set; }
        public int ShiftId { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}