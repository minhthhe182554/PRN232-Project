namespace HRM_API.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ManagerId { get; set; }
        public User Manager { get; set; }
        public ICollection<User> Employees { get; set; }
        public ICollection<SharedFile> SharedFiles { get; set; }
    }
}
