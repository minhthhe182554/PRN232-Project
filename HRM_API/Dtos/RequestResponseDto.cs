using HRM_API.Models.Enums;

namespace HRM_API.Dtos
{
    public class RequestResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public int UserLevel { get; set; }
        public RequestType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Content { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}

