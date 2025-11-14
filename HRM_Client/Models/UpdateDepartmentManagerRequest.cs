using System.ComponentModel.DataAnnotations;

namespace HRM_Client.Models
{
    public class UpdateDepartmentManagerRequest
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "New manager is required")]
        public int? NewManagerId { get; set; }
    }
}

