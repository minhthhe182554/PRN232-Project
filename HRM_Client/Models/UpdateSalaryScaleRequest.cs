using System.ComponentModel.DataAnnotations;

namespace HRM_Client.Models
{
    public class UpdateSalaryScaleRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Base Salary is required")]
        [Range(1000000, 1000000000, ErrorMessage = "Base Salary must be between 1,000,000 and 1,000,000,000")]
        public decimal BaseSalary { get; set; }
    }
}

