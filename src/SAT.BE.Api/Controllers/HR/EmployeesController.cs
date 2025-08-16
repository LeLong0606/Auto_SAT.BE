using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using SAT.BE.src.SAT.BE.Domain.Interfaces;
using SAT.BE.src.SAT.BE.Application.DTOs.Request.Employee;
using SAT.BE.src.SAT.BE.Application.DTOs.Response.Employee;
using SAT.BE.src.SAT.BE.Application.Common;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;

namespace SAT.BE.src.SAT.BE.Api.Controllers.HR
{
    /// <summary>
    /// Employee management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            IEmployeeRepository employeeRepository,
            IMapper mapper,
            ILogger<EmployeesController> logger)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all employees with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of employees</returns>
        [HttpGet]
        public async Task<ActionResult<ServiceResult<PagedResult<EmployeeResponseDto>>>> GetEmployees(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting employees - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var employees = await _employeeRepository.GetPagedAsync(pageNumber, pageSize);
                var employeeDtos = new PagedResult<EmployeeResponseDto>
                {
                    Items = _mapper.Map<List<EmployeeResponseDto>>(employees.Items),
                    TotalCount = employees.TotalCount,
                    PageNumber = employees.PageNumber,
                    PageSize = employees.PageSize
                };

                return Ok(ServiceResult<PagedResult<EmployeeResponseDto>>.Success(employeeDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting employees");
                return StatusCode(500, ServiceResult<PagedResult<EmployeeResponseDto>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>Employee details</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceResult<EmployeeResponseDto>>> GetEmployee(int id)
        {
            try
            {
                _logger.LogInformation("Getting employee with ID: {EmployeeId}", id);

                var employee = await _employeeRepository.GetByIdAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found", id);
                    return NotFound(ServiceResult<EmployeeResponseDto>.Failure("Employee not found.", 404));
                }

                var employeeDto = _mapper.Map<EmployeeResponseDto>(employee);
                return Ok(ServiceResult<EmployeeResponseDto>.Success(employeeDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting employee with ID: {EmployeeId}", id);
                return StatusCode(500, ServiceResult<EmployeeResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get employee by employee code
        /// </summary>
        /// <param name="code">Employee code</param>
        /// <returns>Employee details</returns>
        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<ServiceResult<EmployeeResponseDto>>> GetEmployeeByCode(string code)
        {
            try
            {
                _logger.LogInformation("Getting employee with code: {EmployeeCode}", code);

                var employee = await _employeeRepository.GetByEmployeeCodeAsync(code);
                if (employee == null)
                {
                    _logger.LogWarning("Employee with code {EmployeeCode} not found", code);
                    return NotFound(ServiceResult<EmployeeResponseDto>.Failure("Employee not found.", 404));
                }

                var employeeDto = _mapper.Map<EmployeeResponseDto>(employee);
                return Ok(ServiceResult<EmployeeResponseDto>.Success(employeeDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting employee with code: {EmployeeCode}", code);
                return StatusCode(500, ServiceResult<EmployeeResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Get employees by department
        /// </summary>
        /// <param name="departmentId">Department ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of employees in the department</returns>
        [HttpGet("department/{departmentId:int}")]
        public async Task<ActionResult<ServiceResult<PagedResult<EmployeeResponseDto>>>> GetEmployeesByDepartment(
            int departmentId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting employees for department: {DepartmentId}", departmentId);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var employees = await _employeeRepository.GetByDepartmentAsync(departmentId, pageNumber, pageSize);
                var employeeDtos = new PagedResult<EmployeeResponseDto>
                {
                    Items = _mapper.Map<List<EmployeeResponseDto>>(employees.Items),
                    TotalCount = employees.TotalCount,
                    PageNumber = employees.PageNumber,
                    PageSize = employees.PageSize
                };

                return Ok(ServiceResult<PagedResult<EmployeeResponseDto>>.Success(employeeDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting employees for department: {DepartmentId}", departmentId);
                return StatusCode(500, ServiceResult<PagedResult<EmployeeResponseDto>>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Create a new employee
        /// </summary>
        /// <param name="request">Employee creation request</param>
        /// <returns>Created employee details</returns>
        [HttpPost]
        public async Task<ActionResult<ServiceResult<EmployeeResponseDto>>> CreateEmployee([FromBody] CreateEmployeeRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating employee with code: {EmployeeCode}", request.EmployeeCode);

                // Check if employee code already exists
                var existingEmployee = await _employeeRepository.GetByEmployeeCodeAsync(request.EmployeeCode);
                if (existingEmployee != null)
                {
                    return BadRequest(ServiceResult<EmployeeResponseDto>.Failure("Employee code already exists.", 400));
                }

                var employee = _mapper.Map<Employee>(request);
                var createdEmployee = await _employeeRepository.CreateAsync(employee);
                var employeeDto = _mapper.Map<EmployeeResponseDto>(createdEmployee);

                _logger.LogInformation("Employee created successfully with ID: {EmployeeId}", createdEmployee.EmployeeId);
                return CreatedAtAction(nameof(GetEmployee), new { id = createdEmployee.EmployeeId }, 
                    ServiceResult<EmployeeResponseDto>.Success(employeeDto, "Employee created successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating employee");
                return StatusCode(500, ServiceResult<EmployeeResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Update an employee
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <param name="request">Employee update request</param>
        /// <returns>Updated employee details</returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ServiceResult<EmployeeResponseDto>>> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequestDto request)
        {
            try
            {
                if (id != request.EmployeeId)
                {
                    return BadRequest(ServiceResult<EmployeeResponseDto>.Failure("Employee ID mismatch.", 400));
                }

                _logger.LogInformation("Updating employee with ID: {EmployeeId}", id);

                var existingEmployee = await _employeeRepository.GetByIdAsync(id);
                if (existingEmployee == null)
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found for update", id);
                    return NotFound(ServiceResult<EmployeeResponseDto>.Failure("Employee not found.", 404));
                }

                // Check if employee code already exists for another employee
                var employeeWithCode = await _employeeRepository.GetByEmployeeCodeAsync(request.EmployeeCode);
                if (employeeWithCode != null && employeeWithCode.EmployeeId != id)
                {
                    return BadRequest(ServiceResult<EmployeeResponseDto>.Failure("Employee code already exists.", 400));
                }

                _mapper.Map(request, existingEmployee);
                var updatedEmployee = await _employeeRepository.UpdateAsync(existingEmployee);
                var employeeDto = _mapper.Map<EmployeeResponseDto>(updatedEmployee);

                _logger.LogInformation("Employee updated successfully with ID: {EmployeeId}", id);
                return Ok(ServiceResult<EmployeeResponseDto>.Success(employeeDto, "Employee updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating employee with ID: {EmployeeId}", id);
                return StatusCode(500, ServiceResult<EmployeeResponseDto>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Delete an employee
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ServiceResult<bool>>> DeleteEmployee(int id)
        {
            try
            {
                _logger.LogInformation("Deleting employee with ID: {EmployeeId}", id);

                var exists = await _employeeRepository.ExistsAsync(id);
                if (!exists)
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found for deletion", id);
                    return NotFound(ServiceResult<bool>.Failure("Employee not found.", 404));
                }

                var result = await _employeeRepository.DeleteAsync(id);
                if (result)
                {
                    _logger.LogInformation("Employee deleted successfully with ID: {EmployeeId}", id);
                    return Ok(ServiceResult<bool>.Success(true, "Employee deleted successfully."));
                }

                return BadRequest(ServiceResult<bool>.Failure("Failed to delete employee.", 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting employee with ID: {EmployeeId}", id);
                return StatusCode(500, ServiceResult<bool>.Failure("An internal server error occurred.", 500));
            }
        }

        /// <summary>
        /// Check if employee code exists
        /// </summary>
        /// <param name="code">Employee code to check</param>
        /// <returns>True if code exists, false otherwise</returns>
        [HttpGet("check-code/{code}")]
        public async Task<ActionResult<ServiceResult<bool>>> CheckEmployeeCodeExists(string code)
        {
            try
            {
                var exists = await _employeeRepository.IsEmployeeCodeExistsAsync(code);
                return Ok(ServiceResult<bool>.Success(exists));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking employee code: {EmployeeCode}", code);
                return StatusCode(500, ServiceResult<bool>.Failure("An internal server error occurred.", 500));
            }
        }
    }
}