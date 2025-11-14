using HRM_API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class AttendanceRepository
    {
        private readonly HRMDbContext _context;

        public AttendanceRepository(HRMDbContext context)
        {
            _context = context;
        }

        public async Task<List<Attendance>> GetTodayAttendancesAsync()
        {
            var today = DateTime.Today;
            return await _context.Attendances
                .Where(a => a.CheckIn.Date == today)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetWeekAttendancesAsync()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            
            return await _context.Attendances
                .Where(a => a.CheckIn.Date >= startOfWeek && a.CheckIn.Date < endOfWeek)
                .ToListAsync();
        }

        public async Task<int> GetTotalActiveEmployeesAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive && (u.Role == Models.Enums.Role.Manager || u.Role == Models.Enums.Role.Employee))
                .CountAsync();
        }

        public async Task<Attendance?> GetTodayAttendanceByUserIdAsync(int userId)
        {
            var today = DateTime.Today;
            return await _context.Attendances
                .Where(a => a.UserId == userId && a.CheckIn.Date == today)
                .FirstOrDefaultAsync();
        }

        public async Task<Attendance> CreateAttendanceAsync(Attendance attendance)
        {
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return attendance;
        }

        public async Task<bool> UpdateAttendanceAsync(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Attendance>> GetTodayAttendancesByDepartmentAsync(int departmentId)
        {
            var today = DateTime.Today;
            return await _context.Attendances
                .Include(a => a.User)
                .Where(a => a.CheckIn.Date == today && a.User.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetWeekAttendancesByDepartmentAsync(int departmentId)
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            
            return await _context.Attendances
                .Include(a => a.User)
                .Where(a => a.CheckIn.Date >= startOfWeek && a.CheckIn.Date < endOfWeek && a.User.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetAttendanceHistoryByUserIdAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Attendances
                .Where(a => a.UserId == userId && 
                           a.CheckIn.Date >= startDate.Date && 
                           a.CheckIn.Date <= endDate.Date)
                .OrderByDescending(a => a.CheckIn)
                .ToListAsync();
        }

        public async Task<Attendance?> GetAttendanceByUserIdAndDateAsync(int userId, DateTime date)
        {
            return await _context.Attendances
                .Where(a => a.UserId == userId && a.CheckIn.Date == date.Date)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Attendance>> GetMonthlyAttendancesByUserIdAsync(int userId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            return await _context.Attendances
                .Where(a => a.UserId == userId && 
                           a.CheckIn.Date >= startDate && 
                           a.CheckIn.Date <= endDate)
                .OrderBy(a => a.CheckIn)
                .ToListAsync();
        }
    }
}

