using JobBoardAPI.Enums;

namespace JobBoardAPI.Entities
{
	public class JobApplication
	{
		public int JobApplicationId { get; set; }
		public DateTime AppliedDate { get; set; }
		public ApplicationStatus ApplicationStatus { get; set; }
		public string? CoverLetter { get; set; }
		public User? Applicant { get; set; } // Nav
		public int UserId { get; set; } // FK
		public Job? Job { get; set; } // Nav
		public int JobId { get; set; } // FK
		public string? ResumeUrl { get; set; }

	}
}
