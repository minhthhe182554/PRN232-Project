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
    }
}

