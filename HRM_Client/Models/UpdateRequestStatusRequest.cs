namespace HRM_Client.Models
{
    public class UpdateRequestStatusRequest
    {
        public int RequestId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

