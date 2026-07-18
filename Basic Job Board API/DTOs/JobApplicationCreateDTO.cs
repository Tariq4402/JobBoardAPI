using JobBoardAPI.Entities;
using JobBoardAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace JobBoardAPI.DTOs
{
	public class JobApplicationCreateDTO
	{
		
		public string? CoverLetter { get; set; }

		[Required]
		public int JobId { get; set; } // FK
		public string? ResumeUrl { get; set; }
	}
}
