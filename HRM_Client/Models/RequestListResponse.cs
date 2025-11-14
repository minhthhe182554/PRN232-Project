namespace HRM_Client.Models
{
    public class RequestListResponse
    {
        public List<RequestResponseDto> Requests { get; set; } = new List<RequestResponseDto>();
        public int Count { get; set; }
    }
}

