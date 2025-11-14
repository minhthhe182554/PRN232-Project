using HRM_API.Models.Enums;

namespace HRM_API.Dtos
{
    public class CreateRequestDto
    {
        public RequestType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}

