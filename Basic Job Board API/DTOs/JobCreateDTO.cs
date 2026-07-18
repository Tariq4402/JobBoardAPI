using JobBoardAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace JobBoardAPI.DTOs
{
	public class JobCreateDTO
	{
		[Required(ErrorMessage = "Enter Job Title")]
		[MaxLength(150, ErrorMessage = "Job title cannot exceed 150 characters")]
		public required string JobTitle { get; set; }

		[Required(ErrorMessage = "Enter Job Description")]
		[MaxLength(2000)]
		public required string JobDescription { get; set; }

		[Required(ErrorMessage = "Please Select Job Location")]
		public required string JobLocation { get; set; }
		public JobType JobType { get; set; } // full-time, part-time, contract, etc.

		public decimal MinSalary { get; set; }

		public decimal MaxSalary { get; set; }
	}
}
