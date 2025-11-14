namespace HRM_Client.Models
{
    public class RegisterResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

