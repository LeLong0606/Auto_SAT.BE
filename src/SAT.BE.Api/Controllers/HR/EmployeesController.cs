using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using System.Security.Claims;
using SAT.BE.src.SAT.BE.Domain.Interfaces;
using SAT.BE.src.SAT.BE.Application.DTOs.Request.Employee;
using SAT.BE.src.SAT.BE.Application.DTOs.Response.Employee;
using SAT.BE.src.SAT.BE.Application.Common;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using SAT.BE.src.SAT.BE.Api.Authorization;
using SAT.BE.src.SAT.BE.Domain.Entities.Identity;

namespace SAT.BE.src.SAT.BE.Api.Controllers.HR
{
    /// <summary>
    /// Employee management controller with hierarchical role-based authorization
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
        /// Get all employees with pagination - Employees can view basic info, Team leaders can view their team, Directors can view all
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of employees</returns>
        [HttpGet]
        [HasPermission(PermissionConstants.EMPLOYEE_VIEW)]
        public async Task<ActionResult<ServiceResult<PagedResult<EmployeeResponseDto>>>> GetEmployees(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting employees - Page: {PageNumber}, Size: {PageSize}, User: {UserId}", 
                    pageNumber, pageSize, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                // Apply role-based filtering
                var employees = await GetEmployeesBasedOnUserRole(pageNumber, pageSize);
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
        /// Get employee by ID - Access based on role hierarchy
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>Employee details</returns>
        [HttpGet("{id:int}")]
        [HasPermission(PermissionConstants.EMPLOYEE_VIEW)]
        public async Task<ActionResult<ServiceResult<EmployeeResponseDto>>> GetEmployee(int id)
        {
            try
            {
                _logger.LogInformation("Getting employee with ID: {EmployeeId}", id);

                // Check access rights
                if (!await CanAccessEmployee(id))
                {
                    return Forbid();
                }

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
        [HasPermission(PermissionConstants.EMPLOYEE_VIEW)]
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

                // Check access rights
                if (!await CanAccessEmployee(employee.EmployeeId))
                {
                    return Forbid();
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
        /// Get employees by department - Team leaders can view their department, Directors can view all
        /// </summary>
        /// <param name="departmentId">Department ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of employees in the department</returns>
        [HttpGet("department/{departmentId:int}")]
        [HasPermission(PermissionConstants.EMPLOYEE_VIEW)]
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

                // Check department access
                if (!await CanAccessDepartment(departmentId))
                {
                    return Forbid();
                }

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
        /// Create a new employee - Only HR, Directors, and Admins can create employees
        /// </summary>
        /// <param name="request">Employee creation request</param>
        /// <returns>Created employee details</returns>
        [HttpPost]
        [HasPermission(PermissionConstants.EMPLOYEE_CREATE)]
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
        /// Update an employee - HR, Directors, and Admins can update; Team leaders can update their team members
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <param name="request">Employee update request</param>
        /// <returns>Updated employee details</returns>
        [HttpPut("{id:int}")]
        [HasPermission(PermissionConstants.EMPLOYEE_UPDATE)]
        public async Task<ActionResult<ServiceResult<EmployeeResponseDto>>> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequestDto request)
        {
            try
            {
                if (id != request.EmployeeId)
                {
                    return BadRequest(ServiceResult<EmployeeResponseDto>.Failure("Employee ID mismatch.", 400));
                }

                _logger.LogInformation("Updating employee with ID: {EmployeeId}", id);

                // Check access rights
                if (!await CanModifyEmployee(id))
                {
                    return Forbid();
                }

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
        /// Delete an employee - Only HR, Directors, and Admins can delete employees
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:int}")]
        [HasPermission(PermissionConstants.EMPLOYEE_DELETE)]
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
        [HasPermission(PermissionConstants.EMPLOYEE_VIEW)]
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

        #region Private Helper Methods

        private async Task<PagedResult<Employee>> GetEmployeesBasedOnUserRole(int pageNumber, int pageSize)
        {
            // Super admin, admin, director can view all
            if (User.IsInRole(RoleHierarchy.SUPER_ADMIN) || 
                User.IsInRole(RoleHierarchy.ADMIN) || 
                User.IsInRole(RoleHierarchy.DIRECTOR))
            {
                return await _employeeRepository.GetPagedAsync(pageNumber, pageSize);
            }

            // Team leaders and managers can view their department
            if (User.IsInRole(RoleHierarchy.TEAM_LEADER) || User.IsInRole(RoleHierarchy.MANAGER))
            {
                var departmentIdClaim = User.FindFirst("DepartmentId")?.Value;
                if (int.TryParse(departmentIdClaim, out int departmentId))
                {
                    return await _employeeRepository.GetByDepartmentAsync(departmentId, pageNumber, pageSize);
                }
            }

            // HR can view all
            if (User.IsInRole(RoleHierarchy.HR))
            {
                return await _employeeRepository.GetPagedAsync(pageNumber, pageSize);
            }

            // Regular employees can only view their own record
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(employeeIdClaim, out int employeeId))
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId);
                if (employee != null)
                {
                    return new PagedResult<Employee>(new List<Employee> { employee }, 1, pageNumber, pageSize);
                }
            }

            return new PagedResult<Employee>(new List<Employee>(), 0, pageNumber, pageSize);
        }

        private async Task<bool> CanAccessEmployee(int employeeId)
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
                    var employee = await _employeeRepository.GetByIdAsync(employeeId);
                    return employee?.DepartmentId == userDepartmentId;
                }
            }

            // Employees can access their own record
            var userEmployeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(userEmployeeIdClaim, out int userEmployeeId))
            {
                return userEmployeeId == employeeId;
            }

            return false;
        }

        private async Task<bool> CanModifyEmployee(int employeeId)
        {
            // Super admin, admin, director can modify all
            if (User.IsInRole(RoleHierarchy.SUPER_ADMIN) || 
                User.IsInRole(RoleHierarchy.ADMIN) || 
                User.IsInRole(RoleHierarchy.DIRECTOR))
            {
                return true;
            }

            // HR can modify all
            if (User.IsInRole(RoleHierarchy.HR))
            {
                return true;
            }

            // Team leaders can modify their department employees (but not other team leaders or higher)
            if (User.IsInRole(RoleHierarchy.TEAM_LEADER))
            {
                var userDepartmentIdClaim = User.FindFirst("DepartmentId")?.Value;
                if (int.TryParse(userDepartmentIdClaim, out int userDepartmentId))
                {
                    var employee = await _employeeRepository.GetByIdAsync(employeeId);
                    if (employee?.DepartmentId == userDepartmentId)
                    {
                        // Check if target employee is not a team leader or higher
                        var positionLevel = employee.WorkPosition?.Level ?? 1;
                        return positionLevel < 3; // Below team leader level
                    }
                }
            }

            return false;
        }

        private async Task<bool> CanAccessDepartment(int departmentId)
        {
            // Super admin, admin, director can access all departments
            if (User.IsInRole(RoleHierarchy.SUPER_ADMIN) || 
                User.IsInRole(RoleHierarchy.ADMIN) || 
                User.IsInRole(RoleHierarchy.DIRECTOR))
            {
                return true;
            }

            // HR can access all departments
            if (User.IsInRole(RoleHierarchy.HR))
            {
                return true;
            }

            // Team leaders and managers can access their department
            if (User.IsInRole(RoleHierarchy.TEAM_LEADER) || User.IsInRole(RoleHierarchy.MANAGER))
            {
                var userDepartmentIdClaim = User.FindFirst("DepartmentId")?.Value;
                if (int.TryParse(userDepartmentIdClaim, out int userDepartmentId))
                {
                    return userDepartmentId == departmentId;
                }
            }

            return false;
        }

        #endregion
    }
}