using System.ComponentModel.DataAnnotations;

namespace JobBoardAPI.DTOs
{
	public class UserLoginDTO
	{
		[Required(ErrorMessage = "Please Enter Your Email")]
		[EmailAddress]
		public required string Email { get; set; }

		[Required(ErrorMessage = "Please Enter your Password")]
		[MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
		[MaxLength(80, ErrorMessage = "Password must not exceed 80 characters")]
		public required string Password { get; set; }

	}
}
