using JobBoardAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace JobBoardAPI.DTOs
{
	public class UserUpdateDTO
	{
		[MaxLength(100, ErrorMessage = "Name Must not Exceed 100 Characters")]
		public string? Name { get; set; }

		[MaxLength(500)]
		public string? About { get; set; }

		[MaxLength(100, ErrorMessage = "Company Name Must not Exceed 100 Characters")]
		public string? CompanyName { get; set; }
	}
}
