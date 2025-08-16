using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using SAT.BE.src.SAT.BE.Application.Common;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;

namespace SAT.BE.src.SAT.BE.Api.Controllers.Scheduling
{
    /// <summary>
    /// Shift Assignment management controller
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
        /// Get shift assignments with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="employeeId">Filter by employee ID (optional)</param>
        /// <param name="shiftId">Filter by shift ID (optional)</param>
        /// <param name="date">Filter by date (optional)</param>
        /// <returns>Paginated list of shift assignments</returns>
        [HttpGet]
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
                    .Include(sa => sa.Shift)
                    .AsQueryable();

                // Apply filters
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
                            sa.Employee.Email
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
        public async Task<ActionResult<ServiceResult<List<object>>>> GetEmployeeShiftAssignments(
            int employeeId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting shift assignments for employee: {EmployeeId}", employeeId);

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
        /// Get shift assignment by ID
        /// </summary>
        /// <param name="id">Shift Assignment ID</param>
        /// <returns>Shift assignment details</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceResult<object>>> GetShiftAssignment(int id)
        {
            try
            {
                _logger.LogInformation("Getting shift assignment with ID: {ShiftAssignmentId}", id);

                var assignment = await _context.ShiftAssignments
                    .Include(sa => sa.Employee)
                    .Include(sa => sa.Shift)
                    .Where(sa => sa.ShiftAssignmentId == id)
                    .Select(sa => new
                    {
                        sa.ShiftAssignmentId,
                        sa.Date,
                        sa.Status,
                        sa.StatusCode,
                        sa.Notes,
                        sa.CheckInTime,
                        sa.CheckOutTime,
                        sa.CreatedDate,
                        sa.ModifiedDate,
                        Employee = new
                        {
                            sa.Employee.EmployeeId,
                            sa.Employee.EmployeeCode,
                            sa.Employee.FullName,
                            sa.Employee.Email
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
                    .FirstOrDefaultAsync();

                if (assignment == null)
                {
                    _logger.LogWarning("Shift assignment with ID {ShiftAssignmentId} not found", id);
                    return NotFound(ServiceResult<object>.Failure("Shift assignment not found.", 404));
                }

                return Ok(ServiceResult<object>.Success(assignment));
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
        public async Task<ActionResult<ServiceResult<List<object>>>> GetTodayShiftAssignments()
        {
            try
            {
                _logger.LogInformation("Getting today's shift assignments");

                var today = DateTime.UtcNow.Date;
                var assignments = await _context.ShiftAssignments
                    .Where(sa => sa.Date.Date == today)
                    .Include(sa => sa.Employee)
                    .Include(sa => sa.Shift)
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
                            DepartmentName = sa.Employee.Department.DepartmentName
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
    }
}