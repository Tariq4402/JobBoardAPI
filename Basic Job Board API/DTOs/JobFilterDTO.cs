using JobBoardAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace JobBoardAPI.DTOs
{
	public class JobFilterDTO
	{
		public string? JobTitle { get; set; }
		public string? Location { get; set; }
		public decimal? MinSalary { get; set; }
		public decimal? MaxSalary { get; set; }
		public JobType? JobType { get; set; }
		public JobStatus? JobStatus { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Page Number must be at leat 1")]
		public int Page { get; set; } = 1;

		[Range(1, 50, ErrorMessage = "PageSize Must be between 1 and 50")]
		public int PageSize { get; set; } = 10;
	}
}
