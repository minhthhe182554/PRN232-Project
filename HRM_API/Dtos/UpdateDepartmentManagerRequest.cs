using System.ComponentModel.DataAnnotations;

namespace HRM_API.Dtos
{
    public class UpdateDepartmentManagerRequest
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "New manager is required")]
        public int? NewManagerId { get; set; }
    }
}

