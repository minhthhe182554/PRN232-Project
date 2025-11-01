using HRM_API.Models.Enums;

namespace HRM_API.Models
{
    public class SalaryScale
    {
        public int Id { get; set; }
        public Role Role { get; set; }
        public int Level { get; set; }
        public decimal BaseSalary { get; set; }
        public string? Description { get; set; }
    }
}

