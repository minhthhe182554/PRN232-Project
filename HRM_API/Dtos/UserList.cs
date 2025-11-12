using HRM_API.Models;

namespace HRM_API.Dtos
{
    public class UserList
    {
        public List<UserListDto>? ActiveUsers { get; set; }
        public List<UserListDto>? InactiveUsers { get; set; }
        public int Count { get; set; }
    }
}

