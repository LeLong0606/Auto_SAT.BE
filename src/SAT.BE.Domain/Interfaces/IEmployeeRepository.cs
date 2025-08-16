using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using SAT.BE.src.SAT.BE.Application.Common;

namespace SAT.BE.src.SAT.BE.Domain.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<Employee?> GetByEmployeeCodeAsync(string employeeCode);
        Task<PagedResult<Employee>> GetByDepartmentAsync(int departmentId, int pageNumber, int pageSize);
        Task<List<Employee>> GetByManagerAsync(int managerId);
        Task<bool> IsEmployeeCodeExistsAsync(string employeeCode);
    }

    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}