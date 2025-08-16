using Microsoft.EntityFrameworkCore;
using SAT.BE.src.SAT.BE.Domain.Entities.HR;
using SAT.BE.src.SAT.BE.Domain.Interfaces;
using SAT.BE.src.SAT.BE.Infrastructure.Data;
using SAT.BE.src.SAT.BE.Application.Common;

namespace SAT.BE.src.SAT.BE.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.WorkPosition)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.WorkPosition)
                .Where(e => e.IsActive)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<PagedResult<Employee>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.WorkPosition)
                .Where(e => e.IsActive);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(e => e.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Employee>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<Employee> CreateAsync(Employee entity)
        {
            _context.Employees.Add(entity);
            await _context.SaveChangesAsync();
            
            // Reload with related data
            return await GetByIdAsync(entity.EmployeeId) ?? entity;
        }

        public async Task<Employee> UpdateAsync(Employee entity)
        {
            _context.Employees.Update(entity);
            await _context.SaveChangesAsync();
            
            // Reload with related data
            return await GetByIdAsync(entity.EmployeeId) ?? entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            _context.Employees.Remove(employee);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == id);
        }

        public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.WorkPosition)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task<PagedResult<Employee>> GetByDepartmentAsync(int departmentId, int pageNumber, int pageSize)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.WorkPosition)
                .Where(e => e.DepartmentId == departmentId && e.IsActive);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(e => e.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Employee>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<List<Employee>> GetByManagerAsync(int managerId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.WorkPosition)
                .Where(e => e.Department.LeaderId == managerId && e.IsActive)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<bool> IsEmployeeCodeExistsAsync(string employeeCode)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeCode == employeeCode);
        }
    }
}