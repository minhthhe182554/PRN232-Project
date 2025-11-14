using HRM_API.Models;
using HRM_API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRM_API.Repositories
{
    public class RequestRepository
    {
        private readonly HRMDbContext _context;

        public RequestRepository(HRMDbContext context)
        {
            _context = context;
        }

        public async Task<List<Request>> GetTodayRequestsAsync()
        {
            var today = DateTime.Today;
            return await _context.Requests
                .Where(r => r.StartDate.Date == today || 
                           (r.StartDate.Date <= today && r.EndDate.Date >= today))
                .ToListAsync();
        }

        public async Task<List<Request>> GetWeekRequestsAsync()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            
            return await _context.Requests
                .Where(r => (r.StartDate.Date >= startOfWeek && r.StartDate.Date < endOfWeek) ||
                           (r.StartDate.Date < startOfWeek && r.EndDate.Date >= startOfWeek))
                .ToListAsync();
        }

        public async Task<List<Request>> GetTodayRequestsByDepartmentAsync(int departmentId)
        {
            var today = DateTime.Today;
            return await _context.Requests
                .Include(r => r.User)
                .Where(r => (r.StartDate.Date == today || 
                           (r.StartDate.Date <= today && r.EndDate.Date >= today)) &&
                           r.User.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<List<Request>> GetWeekRequestsByDepartmentAsync(int departmentId)
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            
            return await _context.Requests
                .Include(r => r.User)
                .Where(r => ((r.StartDate.Date >= startOfWeek && r.StartDate.Date < endOfWeek) ||
                           (r.StartDate.Date < startOfWeek && r.EndDate.Date >= startOfWeek)) &&
                           r.User.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<List<Request>> GetRequestsByDepartmentAndTypeAsync(int departmentId, RequestType? type = null)
        {
            var query = _context.Requests
                .Include(r => r.User)
                .Where(r => r.User.DepartmentId == departmentId);

            if (type.HasValue)
            {
                query = query.Where(r => r.Type == type.Value);
            }

            return await query
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<Request?> GetByIdAsync(int id)
        {
            return await _context.Requests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> UpdateRequestStatusAsync(int requestId, RequestStatus status)
        {
            var request = await _context.Requests.FindAsync(requestId);
            if (request == null)
                return false;

            request.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Request>> GetRequestsByUserIdAsync(int userId)
        {
            return await _context.Requests
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<Request> CreateRequestAsync(Request request)
        {
            _context.Requests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }
    }
}

