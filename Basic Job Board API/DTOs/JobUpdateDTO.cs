using JobBoardAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace JobBoardAPI.DTOs
{
	public class JobUpdateDTO
	{
		[MaxLength(150, ErrorMessage = "Job Title cannot Exceed 150 Characters")]
		public string? JobTitle { get; set; }

		[MaxLength(2000)]
		public string? JobDescription { get; set; }
		public string? JobLocation { get; set; }
		public JobType? JobType { get; set; } // full-time, part-time, contract, etc.
		public decimal? MinSalary { get; set; }
		public decimal? MaxSalary { get; set; }
		public JobStatus? JobStatus { get; set; }
	}
}
