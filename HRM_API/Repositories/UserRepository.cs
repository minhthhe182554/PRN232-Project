using HRM_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class UserRepository
    {
        private readonly HRMDbContext _context;

        public UserRepository(HRMDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<User?> GetByUsernameIncludingInactiveAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetAllWithDetailsAsync()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.ManagedDepartment)
                .ToListAsync();
            
            return users;
        }

        public async Task<User?> GetByIdIncludingInactiveAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> UpdateUserStatusAsync(int id, bool isActive)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePasswordAsync(int id, string hashedPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Password = hashedPassword;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateRoleAndLevelAsync(int userId, Models.Enums.Role role, int level)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Role = role;
            user.Level = level;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDepartmentAsync(int userId, int departmentId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.DepartmentId = departmentId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> SearchEmployeesByUsernameAsync(string username, int? departmentId = null)
        {
            var query = _context.Users
                .Include(u => u.Department)
                .Where(u => u.IsActive 
                    && u.Role == Models.Enums.Role.Employee 
                    && u.Username.Contains(username));

            if (departmentId.HasValue)
            {
                query = query.Where(u => u.DepartmentId == departmentId.Value);
            }

            return await query.Take(10).ToListAsync();
        }

        public async Task<List<User>> GetEmployeesByDepartmentAsync(int departmentId)
        {
            return await _context.Users
                .Include(u => u.Department)
                .Where(u => u.IsActive 
                    && u.Role == Models.Enums.Role.Employee 
                    && u.DepartmentId == departmentId)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserProfileAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            return await _context.Users
                .Where(u => u.DepartmentId == departmentId && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<int> GetDepartmentEmployeeCountAsync(int departmentId)
        {
            return await _context.Users
                .Where(u => u.DepartmentId == departmentId && u.IsActive)
                .CountAsync();
        }

        public async Task<bool> UpdateLevelAsync(int userId, int newLevel)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Level = newLevel;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

