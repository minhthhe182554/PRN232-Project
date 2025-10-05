namespace HumanResourceManagement.Models
{
	public class SharedFile
	{
		public int Id { get; set; }
		public string FileName { get; set; }
		public string FileUrl { get; set; } // Firebase link
		public DateTime UploadedAt { get; set; }
		public int DepartmentId { get; set; }
		public Department Department { get; set; }
		public string UploadedById { get; set; }
		public User UploadedBy { get; set; }
	}
}
