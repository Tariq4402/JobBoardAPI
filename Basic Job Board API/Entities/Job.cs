using JobBoardAPI.Enums;
namespace JobBoardAPI.Entities
{
	public class Job
	{
		public int JobId { get; set; }
		public required string JobTitle { get; set; }
		public required string JobDescription { get; set; }
		public required string JobLocation { get; set; }
		public JobType JobType { get; set; } // full-time, part-time, contract, etc.
		public decimal MinSalary { get; set; }
		public decimal MaxSalary { get; set; }
		public DateTime PostedDate { get; set; }
		public JobStatus JobStatus { get; set; }
		public User? Employer { get; set; } // Nav
		public int UserId { get; set; } // FK

	}
}
