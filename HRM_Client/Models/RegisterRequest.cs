using System.ComponentModel.DataAnnotations;

namespace HRM_Client.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Middle name is required")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MinLength(5, ErrorMessage = "Address must be at least 5 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Level is required")]
        [Range(1, 3, ErrorMessage = "Level must be between 1 and 3")]
        public int Level { get; set; } = 1;

        public int? DepartmentId { get; set; }
    }
}

