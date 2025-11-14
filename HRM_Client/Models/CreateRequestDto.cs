namespace HRM_Client.Models
{
    public class CreateRequestDto
    {
        public string Type { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}

