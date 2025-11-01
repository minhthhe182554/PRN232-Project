using HRM_API.Models.Enums;

namespace HRM_API.Dtos
{
    public class RegisterRequest
    {
        public required string FirstName { get; set; } 
        public required string MiddleName { get; set; } 
        public required string Address { get; set; }
        public Role Role { get; set; }
        public int Level { get; set; } = 1;
        public int? DepartmentId { get; set; }
        public string? ProfileImgUrl { get; set; }
        public int AnnualLeaveDays { get; set; } = 12;
    }
}
