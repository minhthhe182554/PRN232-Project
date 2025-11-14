using System.ComponentModel.DataAnnotations;

namespace HRM_API.Dtos
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [MinLength(2, ErrorMessage = "Full name must be at least 2 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MinLength(5, ErrorMessage = "Address must be at least 5 characters")]
        public string Address { get; set; } = string.Empty;
    }
}

