namespace HRM_Client.Models
{
    public class BanUserResponse
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

