using System.ComponentModel.DataAnnotations;

namespace JobBoardAPI.DTOs
{
	public class EmailUpdateDTO
	{
		[Required(ErrorMessage = "Please Enter the Email")]
		[EmailAddress]
		public required string Email { get; set; }
	}
}
