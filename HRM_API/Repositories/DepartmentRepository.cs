using HRM_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class DepartmentRepository
    {
        private readonly HRMDbContext _context;

        public DepartmentRepository(HRMDbContext context)
        {
            _context = context;
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _context.Departments
                .Include(d => d.Manager)
                .Include(d => d.Employees)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _context.Departments
                .Include(d => d.Manager)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> UpdateNameAsync(int id, string name)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return false;

            department.Name = name;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateManagerAsync(int departmentId, int managerId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null) return false;

            department.ManagerId = managerId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (department == null) return false;
            
            // Check if department has employees
            if (department.Employees != null && department.Employees.Any())
                return false;

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetEmployeeCountAsync(int departmentId)
        {
            return await _context.Users
                .Where(u => u.DepartmentId == departmentId && u.IsActive)
                .CountAsync();
        }

        public async Task<Department> CreateAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
            return department;
        }
    }
}

