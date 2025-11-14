using System.ComponentModel.DataAnnotations;

namespace HRM_API.Dtos
{
    public class SearchEmployeeRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(2, ErrorMessage = "Username must be at least 2 characters")]
        public string? Username { get; set; }
    }
}

