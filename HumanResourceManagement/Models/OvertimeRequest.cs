namespace HumanResourceManagement.Models
{
	public class OvertimeRequest
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public User User { get; set; }
		public DateTime Date { get; set; }
		public int Hours { get; set; }
		public string Reason { get; set; }
		public RequestStatus Status { get; set; } = RequestStatus.Pending;
	}
}
