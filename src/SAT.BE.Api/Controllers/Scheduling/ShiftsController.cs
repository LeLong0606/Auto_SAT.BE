using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using SAT.BE.src.SAT.BE.Application.Common;
using SAT.BE.src.SAT.BE.Domain.Entities.Scheduling;

namespace SAT.BE.src.SAT.BE.Api.Controllers.Scheduling
{
    /// <summary>
    /// Shift management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ShiftsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShiftsController> _logger;

        public ShiftsController(
            ApplicationDbContext context,
            ILogger<ShiftsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all shifts
        /// </summary>
        /// <param name="includeInactive">Include inactive shifts (default: false)</param>
        /// <returns>List of shifts</returns>
        [HttpGet]
        public async Task<ActionResult<ServiceResult<List<Shift>>>> GetShifts([FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting shifts");

                var query = _context.Shifts.AsQueryable();

                if (!includeInactive)
                {
                    query = query.Where(s => s.IsActive);
                }

                var shifts = await query
                    .OrderBy(s => s.Type)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();

                return Ok(ServiceResult<List<Shift>>.Success(shifts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting shifts");
                return StatusCode(500, ServiceResult<List<Shift>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get shift by ID
        /// </summary>
        /// <param name="id">Shift ID</param>
        /// <returns>Shift details</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceResult<Shift>>> GetShift(int id)
        {
            try
            {
                _logger.LogInformation("Getting shift with ID: {ShiftId}", id);

                var shift = await _context.Shifts
                    .FirstOrDefaultAsync(s => s.ShiftId == id);

                if (shift == null)
                {
                    _logger.LogWarning("Shift with ID {ShiftId} not found", id);
                    return NotFound(ServiceResult<Shift>.Failure("Shift not found.", 404));
                }

                return Ok(ServiceResult<Shift>.Success(shift));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting shift with ID: {ShiftId}", id);
                return StatusCode(500, ServiceResult<Shift>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get shifts by type
        /// </summary>
        /// <param name="type">Shift type (Morning=0, Afternoon=1, Night=2)</param>
        /// <returns>List of shifts of the specified type</returns>
        [HttpGet("by-type/{type:int}")]
        public async Task<ActionResult<ServiceResult<List<Shift>>>> GetShiftsByType(ShiftType type)
        {
            try
            {
                _logger.LogInformation("Getting shifts with type: {ShiftType}", type);

                var shifts = await _context.Shifts
                    .Where(s => s.Type == type && s.IsActive)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                return Ok(ServiceResult<List<Shift>>.Success(shifts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting shifts with type: {ShiftType}", type);
                return StatusCode(500, ServiceResult<List<Shift>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get active shifts (for dropdown lists)
        /// </summary>
        /// <returns>List of active shifts</returns>
        [HttpGet("active")]
        public async Task<ActionResult<ServiceResult<List<Shift>>>> GetActiveShifts()
        {
            try
            {
                _logger.LogInformation("Getting active shifts");

                var shifts = await _context.Shifts
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Type)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();

                return Ok(ServiceResult<List<Shift>>.Success(shifts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting active shifts");
                return StatusCode(500, ServiceResult<List<Shift>>.Failure("An internal server error occurred.", 500));
            }
        }
    }
}