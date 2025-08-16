using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using SAT.BE.src.SAT.BE.Application.Common;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using TaskStatusEnum = SAT.BE.src.SAT.BE.Domain.Entities.HR.TaskStatus;

namespace SAT.BE.src.SAT.BE.Api.Controllers
{
    /// <summary>
    /// Dashboard statistics controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        /// <returns>Dashboard statistics including employee counts, department counts, etc.</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<ServiceResult<object>>> GetDashboardStatistics()
        {
            try
            {
                _logger.LogInformation("Getting dashboard statistics");

                var today = DateTime.UtcNow.Date;

                var statistics = new
                {
                    TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive),
                    TotalDepartments = await _context.Departments.CountAsync(d => d.IsActive),
                    TotalWorkPositions = await _context.WorkPositions.CountAsync(wp => wp.IsActive),
                    TotalUsers = await _context.Users.CountAsync(u => u.IsActive),
                    TodayShiftAssignments = await _context.ShiftAssignments.CountAsync(sa => sa.Date.Date == today),
                    ActiveTaskAssignments = await _context.TaskAssignments.CountAsync(ta => ta.Status == TaskStatusEnum.InProgress),
                    CompletedTasksThisMonth = await _context.TaskAssignments
                        .CountAsync(ta => ta.Status == TaskStatusEnum.Completed && 
                                         ta.CompletedDate.HasValue && 
                                         ta.CompletedDate.Value.Month == DateTime.UtcNow.Month &&
                                         ta.CompletedDate.Value.Year == DateTime.UtcNow.Year),
                    RecentEmployees = await _context.Employees
                        .Where(e => e.IsActive)
                        .OrderByDescending(e => e.CreatedDate)
                        .Take(5)
                        .Select(e => new
                        {
                            e.EmployeeId,
                            e.EmployeeCode,
                            e.FullName,
                            e.Email,
                            DepartmentName = e.Department.DepartmentName,
                            PositionName = e.WorkPosition.PositionName,
                            e.CreatedDate
                        })
                        .ToListAsync()
                };

                return Ok(ServiceResult<object>.Success(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting dashboard statistics");
                return StatusCode(500, ServiceResult<object>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get employee statistics by department
        /// </summary>
        /// <returns>Employee count grouped by department</returns>
        [HttpGet("employee-by-department")]
        public async Task<ActionResult<ServiceResult<List<object>>>> GetEmployeesByDepartment()
        {
            try
            {
                _logger.LogInformation("Getting employee statistics by department");

                var departmentStats = await _context.Departments
                    .Where(d => d.IsActive)
                    .Select(d => new
                    {
                        DepartmentId = d.DepartmentId,
                        DepartmentName = d.DepartmentName,
                        DepartmentCode = d.DepartmentCode,
                        EmployeeCount = d.Employees.Count(e => e.IsActive),
                        LeaderName = d.Leader != null ? d.Leader.FullName : null
                    })
                    .OrderByDescending(d => d.EmployeeCount)
                    .ToListAsync();

                return Ok(ServiceResult<List<object>>.Success(departmentStats.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting employee statistics by department");
                return StatusCode(500, ServiceResult<List<object>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get employee statistics by work position
        /// </summary>
        /// <returns>Employee count grouped by work position</returns>
        [HttpGet("employee-by-position")]
        public async Task<ActionResult<ServiceResult<List<object>>>> GetEmployeesByPosition()
        {
            try
            {
                _logger.LogInformation("Getting employee statistics by work position");

                var positionStats = await _context.WorkPositions
                    .Where(wp => wp.IsActive)
                    .Select(wp => new
                    {
                        WorkPositionId = wp.WorkPositionId,
                        PositionName = wp.PositionName,
                        PositionCode = wp.PositionCode,
                        Level = wp.Level,
                        EmployeeCount = wp.Employees.Count(e => e.IsActive),
                        BaseSalary = wp.BaseSalary
                    })
                    .OrderBy(wp => wp.Level)
                    .ThenByDescending(wp => wp.EmployeeCount)
                    .ToListAsync();

                return Ok(ServiceResult<List<object>>.Success(positionStats.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting employee statistics by work position");
                return StatusCode(500, ServiceResult<List<object>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get attendance summary for today
        /// </summary>
        /// <returns>Today's attendance statistics</returns>
        [HttpGet("attendance-today")]
        public async Task<ActionResult<ServiceResult<object>>> GetTodayAttendance()
        {
            try
            {
                _logger.LogInformation("Getting today's attendance summary");

                var today = DateTime.UtcNow.Date;
                var attendanceStats = new
                {
                    Date = today,
                    TotalScheduled = await _context.ShiftAssignments.CountAsync(sa => sa.Date.Date == today),
                    CheckedIn = await _context.ShiftAssignments.CountAsync(sa => sa.Date.Date == today && sa.CheckInTime.HasValue),
                    CheckedOut = await _context.ShiftAssignments.CountAsync(sa => sa.Date.Date == today && sa.CheckOutTime.HasValue),
                    OnLeave = await _context.ShiftAssignments.CountAsync(sa => sa.Date.Date == today && sa.StatusCode == "LE"),
                    Absent = await _context.ShiftAssignments.CountAsync(sa => sa.Date.Date == today && sa.StatusCode == "AB"),
                    ByShiftType = await _context.ShiftAssignments
                        .Where(sa => sa.Date.Date == today)
                        .GroupBy(sa => sa.Shift.Type)
                        .Select(g => new
                        {
                            ShiftType = g.Key,
                            Count = g.Count(),
                            CheckedIn = g.Count(sa => sa.CheckInTime.HasValue)
                        })
                        .ToListAsync()
                };

                return Ok(ServiceResult<object>.Success(attendanceStats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting today's attendance summary");
                return StatusCode(500, ServiceResult<object>.Failure("An internal server error occurred.", 500));
            }
        }
    }
}