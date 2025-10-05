namespace HumanResourceManagement.Models
{
	public class Department
	{
		public int Id { get; set; }
		public string Name { get; set; }

		// 1 phòng ban có 1 manager
		public string ManagerId { get; set; }
		public User Manager { get; set; }

		public ICollection<User> Employees { get; set; }

		public ICollection<SharedFile> SharedFiles { get; set; }
	}
}
