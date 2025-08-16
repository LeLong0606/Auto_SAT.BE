using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using SAT.BE.src.SAT.BE.Application.DTOs.Request.Department;
using SAT.BE.src.SAT.BE.Application.DTOs.Response.Department;
using SAT.BE.src.SAT.BE.Application.Common;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Api.Controllers.HR
{
    /// <summary>
    /// Department management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<DepartmentsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all departments with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="includeInactive">Include inactive departments (default: false)</param>
        /// <returns>Paginated list of departments</returns>
        [HttpGet]
        public async Task<ActionResult<ServiceResult<PagedResult<DepartmentResponseDto>>>> GetDepartments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting departments - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = _context.Departments
                    .Include(d => d.Leader)
                    .Include(d => d.Employees)
                    .AsQueryable();

                if (!includeInactive)
                {
                    query = query.Where(d => d.IsActive);
                }

                var totalCount = await query.CountAsync();
                var departments = await query
                    .OrderBy(d => d.DepartmentName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var departmentDtos = _mapper.Map<List<DepartmentResponseDto>>(departments);
                var result = new PagedResult<DepartmentResponseDto>(departmentDtos, totalCount, pageNumber, pageSize);

                return Ok(ServiceResult<PagedResult<DepartmentResponseDto>>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting departments");
                return StatusCode(500, ServiceResult<PagedResult<DepartmentResponseDto>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get all active departments (for dropdown lists)
        /// </summary>
        /// <returns>List of active departments</returns>
        [HttpGet("active")]
        public async Task<ActionResult<ServiceResult<List<DepartmentResponseDto>>>> GetActiveDepartments()
        {
            try
            {
                _logger.LogInformation("Getting active departments");

                var departments = await _context.Departments
                    .Where(d => d.IsActive)
                    .Include(d => d.Leader)
                    .Include(d => d.Employees)
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();

                var departmentDtos = _mapper.Map<List<DepartmentResponseDto>>(departments);
                return Ok(ServiceResult<List<DepartmentResponseDto>>.Success(departmentDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting active departments");
                return StatusCode(500, ServiceResult<List<DepartmentResponseDto>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get department by ID
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <returns>Department details</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceResult<DepartmentResponseDto>>> GetDepartment(int id)
        {
            try
            {
                _logger.LogInformation("Getting department with ID: {DepartmentId}", id);

                var department = await _context.Departments
                    .Include(d => d.Leader)
                    .Include(d => d.Employees)
                    .FirstOrDefaultAsync(d => d.DepartmentId == id);

                if (department == null)
                {
                    _logger.LogWarning("Department with ID {DepartmentId} not found", id);
                    return NotFound(ServiceResult<DepartmentResponseDto>.Failure("Department not found.", 404));
                }

                var departmentDto = _mapper.Map<DepartmentResponseDto>(department);
                return Ok(ServiceResult<DepartmentResponseDto>.Success(departmentDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting department with ID: {DepartmentId}", id);
                return StatusCode(500, ServiceResult<DepartmentResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get department by department code
        /// </summary>
        /// <param name="code">Department code</param>
        /// <returns>Department details</returns>
        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<ServiceResult<DepartmentResponseDto>>> GetDepartmentByCode(string code)
        {
            try
            {
                _logger.LogInformation("Getting department with code: {DepartmentCode}", code);

                var department = await _context.Departments
                    .Include(d => d.Leader)
                    .Include(d => d.Employees)
                    .FirstOrDefaultAsync(d => d.DepartmentCode == code);

                if (department == null)
                {
                    _logger.LogWarning("Department with code {DepartmentCode} not found", code);
                    return NotFound(ServiceResult<DepartmentResponseDto>.Failure("Department not found.", 404));
                }

                var departmentDto = _mapper.Map<DepartmentResponseDto>(department);
                return Ok(ServiceResult<DepartmentResponseDto>.Success(departmentDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting department with code: {DepartmentCode}", code);
                return StatusCode(500, ServiceResult<DepartmentResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Create a new department
        /// </summary>
        /// <param name="request">Department creation request</param>
        /// <returns>Created department details</returns>
        [HttpPost]
        public async Task<ActionResult<ServiceResult<DepartmentResponseDto>>> CreateDepartment([FromBody] CreateDepartmentRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating department with code: {DepartmentCode}", request.DepartmentCode);

                // Check if department code already exists
                var existingDepartment = await _context.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentCode == request.DepartmentCode);
                if (existingDepartment != null)
                {
                    return BadRequest(ServiceResult<DepartmentResponseDto>.Failure("Department code already exists.", 400));
                }

                // Validate leader exists if provided
                if (request.LeaderId.HasValue)
                {
                    var leaderExists = await _context.Employees
                        .AnyAsync(e => e.EmployeeId == request.LeaderId.Value && e.IsActive);
                    if (!leaderExists)
                    {
                        return BadRequest(ServiceResult<DepartmentResponseDto>.Failure("Specified leader does not exist or is inactive.", 400));
                    }
                }

                var department = _mapper.Map<Department>(request);
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                // Reload department with related data
                var createdDepartment = await _context.Departments
                    .Include(d => d.Leader)
                    .Include(d => d.Employees)
                    .FirstAsync(d => d.DepartmentId == department.DepartmentId);

                var departmentDto = _mapper.Map<DepartmentResponseDto>(createdDepartment);

                _logger.LogInformation("Department created successfully with ID: {DepartmentId}", department.DepartmentId);
                return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId },
                    ServiceResult<DepartmentResponseDto>.Success(departmentDto, "Department created successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating department");
                return StatusCode(500, ServiceResult<DepartmentResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Update a department
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <param name="request">Department update request</param>
        /// <returns>Updated department details</returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ServiceResult<DepartmentResponseDto>>> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequestDto request)
        {
            try
            {
                if (id != request.DepartmentId)
                {
                    return BadRequest(ServiceResult<DepartmentResponseDto>.Failure("Department ID mismatch.", 400));
                }

                _logger.LogInformation("Updating department with ID: {DepartmentId}", id);

                var existingDepartment = await _context.Departments
                    .Include(d => d.Leader)
                    .Include(d => d.Employees)
                    .FirstOrDefaultAsync(d => d.DepartmentId == id);

                if (existingDepartment == null)
                {
                    _logger.LogWarning("Department with ID {DepartmentId} not found for update", id);
                    return NotFound(ServiceResult<DepartmentResponseDto>.Failure("Department not found.", 404));
                }

                // Check if department code already exists for another department
                var departmentWithCode = await _context.Departments
                    .FirstOrDefaultAsync(d => d.DepartmentCode == request.DepartmentCode && d.DepartmentId != id);
                if (departmentWithCode != null)
                {
                    return BadRequest(ServiceResult<DepartmentResponseDto>.Failure("Department code already exists.", 400));
                }

                // Validate leader exists if provided
                if (request.LeaderId.HasValue)
                {
                    var leaderExists = await _context.Employees
                        .AnyAsync(e => e.EmployeeId == request.LeaderId.Value && e.IsActive);
                    if (!leaderExists)
                    {
                        return BadRequest(ServiceResult<DepartmentResponseDto>.Failure("Specified leader does not exist or is inactive.", 400));
                    }
                }

                _mapper.Map(request, existingDepartment);
                existingDepartment.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var departmentDto = _mapper.Map<DepartmentResponseDto>(existingDepartment);

                _logger.LogInformation("Department updated successfully with ID: {DepartmentId}", id);
                return Ok(ServiceResult<DepartmentResponseDto>.Success(departmentDto, "Department updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating department with ID: {DepartmentId}", id);
                return StatusCode(500, ServiceResult<DepartmentResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Delete a department
        /// </summary>
        /// <param name="id">Department ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ServiceResult<bool>>> DeleteDepartment(int id)
        {
            try
            {
                _logger.LogInformation("Deleting department with ID: {DepartmentId}", id);

                var department = await _context.Departments
                    .Include(d => d.Employees)
                    .FirstOrDefaultAsync(d => d.DepartmentId == id);

                if (department == null)
                {
                    _logger.LogWarning("Department with ID {DepartmentId} not found for deletion", id);
                    return NotFound(ServiceResult<bool>.Failure("Department not found.", 404));
                }

                // Check if department has employees
                if (department.Employees.Any())
                {
                    return BadRequest(ServiceResult<bool>.Failure("Cannot delete department with existing employees.", 400));
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Department deleted successfully with ID: {DepartmentId}", id);
                return Ok(ServiceResult<bool>.Success(true, "Department deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting department with ID: {DepartmentId}", id);
                return StatusCode(500, ServiceResult<bool>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Check if department code exists
        /// </summary>
        /// <param name="code">Department code to check</param>
        /// <returns>True if code exists, false otherwise</returns>
        [HttpGet("check-code/{code}")]
        public async Task<ActionResult<ServiceResult<bool>>> CheckDepartmentCodeExists(string code)
        {
            try
            {
                var exists = await _context.Departments.AnyAsync(d => d.DepartmentCode == code);
                return Ok(ServiceResult<bool>.Success(exists));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking department code: {DepartmentCode}", code);
                return StatusCode(500, ServiceResult<bool>.Failure("An internal server error occurred.", 500));
            }
        }
    }
}