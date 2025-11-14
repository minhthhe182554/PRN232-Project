using HRM_API.Models.Enums;

namespace HRM_API.Models
{
    public class Request
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? DepartmentId { get; set; }
        public User User { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RequestType Type { get; set; }
        public string Content { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }
}
