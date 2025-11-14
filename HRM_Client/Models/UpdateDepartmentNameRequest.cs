using System.ComponentModel.DataAnnotations;

namespace HRM_Client.Models
{
    public class UpdateDepartmentNameRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [MinLength(2, ErrorMessage = "Department name must be at least 2 characters")]
        [MaxLength(100, ErrorMessage = "Department name must not exceed 100 characters")]
        public string? Name { get; set; }
    }
}

