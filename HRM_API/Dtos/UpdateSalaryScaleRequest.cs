using System.ComponentModel.DataAnnotations;

namespace HRM_API.Dtos
{
    public class UpdateSalaryScaleRequest
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Base salary is required")]
        [Range(0, 999999999, ErrorMessage = "Base salary must be between 0 and 999,999,999")]
        public decimal BaseSalary { get; set; }
    }
}

