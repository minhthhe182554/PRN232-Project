namespace HumanResourceManagement.Models
{
	public class LeaveRequest
	{
		public int Id { get; set; }

		public string UserId { get; set; }
		public User User { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public LeaveRequestType Type { get; set; } 
		public string Reason { get; set; }
		public RequestStatus Status { get; set; } = RequestStatus.Pending;
	}

	public enum LeaveRequestType
	{
		Normal = 1,   
		Resignation = 2
	}

	public enum RequestStatus
	{
		Pending = 1,
		Approved = 2,
		Rejected = 3
	}
}
