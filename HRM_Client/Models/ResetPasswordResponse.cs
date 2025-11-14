namespace HRM_Client.Models
{
    public class ResetPasswordResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

