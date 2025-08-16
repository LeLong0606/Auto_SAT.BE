using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using SAT.BE.src.SAT.BE.Application.Common;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Api.Controllers.HR
{
    /// <summary>
    /// Work Position management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class WorkPositionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkPositionsController> _logger;

        public WorkPositionsController(
            ApplicationDbContext context,
            ILogger<WorkPositionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all work positions with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="includeInactive">Include inactive positions (default: false)</param>
        /// <returns>Paginated list of work positions</returns>
        [HttpGet]
        public async Task<ActionResult<ServiceResult<PagedResult<WorkPosition>>>> GetWorkPositions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting work positions - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.WorkPositions.AsQueryable();

                if (!includeInactive)
                {
                    query = query.Where(wp => wp.IsActive);
                }

                var totalCount = await query.CountAsync();
                var positions = await query
                    .OrderBy(wp => wp.Level)
                    .ThenBy(wp => wp.PositionName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResult<WorkPosition>(positions, totalCount, pageNumber, pageSize);
                return Ok(ServiceResult<PagedResult<WorkPosition>>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting work positions");
                return StatusCode(500, ServiceResult<PagedResult<WorkPosition>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get all active work positions (for dropdown lists)
        /// </summary>
        /// <returns>List of active work positions</returns>
        [HttpGet("active")]
        public async Task<ActionResult<ServiceResult<List<WorkPosition>>>> GetActiveWorkPositions()
        {
            try
            {
                _logger.LogInformation("Getting active work positions");

                var positions = await _context.WorkPositions
                    .Where(wp => wp.IsActive)
                    .OrderBy(wp => wp.Level)
                    .ThenBy(wp => wp.PositionName)
                    .ToListAsync();

                return Ok(ServiceResult<List<WorkPosition>>.Success(positions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting active work positions");
                return StatusCode(500, ServiceResult<List<WorkPosition>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get work position by ID
        /// </summary>
        /// <param name="id">Work Position ID</param>
        /// <returns>Work position details</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceResult<WorkPosition>>> GetWorkPosition(int id)
        {
            try
            {
                _logger.LogInformation("Getting work position with ID: {WorkPositionId}", id);

                var position = await _context.WorkPositions
                    .FirstOrDefaultAsync(wp => wp.WorkPositionId == id);

                if (position == null)
                {
                    _logger.LogWarning("Work position with ID {WorkPositionId} not found", id);
                    return NotFound(ServiceResult<WorkPosition>.Failure("Work position not found.", 404));
                }

                return Ok(ServiceResult<WorkPosition>.Success(position));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting work position with ID: {WorkPositionId}", id);
                return StatusCode(500, ServiceResult<WorkPosition>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get work position by position code
        /// </summary>
        /// <param name="code">Position code</param>
        /// <returns>Work position details</returns>
        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<ServiceResult<WorkPosition>>> GetWorkPositionByCode(string code)
        {
            try
            {
                _logger.LogInformation("Getting work position with code: {PositionCode}", code);

                var position = await _context.WorkPositions
                    .FirstOrDefaultAsync(wp => wp.PositionCode == code);

                if (position == null)
                {
                    _logger.LogWarning("Work position with code {PositionCode} not found", code);
                    return NotFound(ServiceResult<WorkPosition>.Failure("Work position not found.", 404));
                }

                return Ok(ServiceResult<WorkPosition>.Success(position));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting work position with code: {PositionCode}", code);
                return StatusCode(500, ServiceResult<WorkPosition>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get work positions by level
        /// </summary>
        /// <param name="level">Position level</param>
        /// <returns>List of work positions at the specified level</returns>
        [HttpGet("by-level/{level:int}")]
        public async Task<ActionResult<ServiceResult<List<WorkPosition>>>> GetWorkPositionsByLevel(int level)
        {
            try
            {
                _logger.LogInformation("Getting work positions with level: {Level}", level);

                var positions = await _context.WorkPositions
                    .Where(wp => wp.Level == level && wp.IsActive)
                    .OrderBy(wp => wp.PositionName)
                    .ToListAsync();

                return Ok(ServiceResult<List<WorkPosition>>.Success(positions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting work positions with level: {Level}", level);
                return StatusCode(500, ServiceResult<List<WorkPosition>>.Failure("An internal server error occurred.", 500));
            }
        }
    }
}