namespace HRM_API.Models
{
    public class SharedFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; } 
        public DateTime UploadedAt { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        public int UploadedById { get; set; }
        public User UploadedBy { get; set; }
    }
}
