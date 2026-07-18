using JobBoardAPI.Enums;

namespace JobBoardAPI.DTOs
{
	public class JobResponseDTO
	{
		public int JobId { get; set; }
		public required string JobTitle { get; set; }
		public required string JobDescription { get; set; }
		public required string JobLocation { get; set; }
		public JobType JobType { get; set; } // full-time, part-time, contract, etc.
		public DateTime PostedDate { get; set; }
		public decimal MinSalary { get; set; }
		public decimal MaxSalary { get; set; }
		public JobStatus JobStatus { get; set; }
		public string? Company { get; set; } // Employer's name

	}
}
