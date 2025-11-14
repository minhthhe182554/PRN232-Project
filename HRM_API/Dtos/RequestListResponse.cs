namespace HRM_API.Dtos
{
    public class RequestListResponse
    {
        public List<RequestResponseDto> Requests { get; set; } = new List<RequestResponseDto>();
        public int Count { get; set; }
    }
}

