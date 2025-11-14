namespace HRM_API.Dtos
{
    public class BanUserRequest
    {
        public int UserId { get; set; }
        public bool IsBanned { get; set; } // true = ban, false = unban
    }
}

