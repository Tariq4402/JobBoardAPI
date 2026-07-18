using JobBoardAPI.Entities;
using JobBoardAPI.Enums;

namespace JobBoardAPI.DTOs
{
	public class JobApplicationResponseDTO
	{

		public required string ApplicantName { get; set; }
		public int JobApplicationId { get; set; }
		public DateTime AppliedDate { get; set; }
		public ApplicationStatus ApplicationStatus { get; set; }
		public string? ApplicantEmail { get; set; }
		public string? CoverLetter { get; set; }
		//public int UserId { get; set; } // FK
		//public int JobId { get; set; } // FK
		public required string JobTitle { get; set; }
		public string? ResumeUrl { get; set; }

	}
}
