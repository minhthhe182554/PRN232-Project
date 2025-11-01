using HRM_API.Models;
using HRM_API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class SalaryScaleRepository
    {
        private readonly HRMDbContext _context;

        public SalaryScaleRepository(HRMDbContext context)
        {
            _context = context;
        }

        public async Task<SalaryScale?> GetSalaryAsync(Role role, int level)
        {
            return await _context.SalaryScales
                .FirstOrDefaultAsync(s => s.Role == role && s.Level == level);
        }

        public async Task<decimal> GetBaseSalaryAsync(Role role, int level)
        {
            var scale = await GetSalaryAsync(role, level);
            return scale?.BaseSalary ?? 0;
        }

        public async Task<List<SalaryScale>> GetAllAsync()
        {
            return await _context.SalaryScales
                .OrderBy(s => s.Role)
                .ThenBy(s => s.Level)
                .ToListAsync();
        }
    }
}

