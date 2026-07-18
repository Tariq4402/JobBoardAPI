using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace JobBoardAPI.DTOs
{
	public class UserRegistrationDTO
	{
		[Required(ErrorMessage = ("Enter Your Name"))]
		[MaxLength(100, ErrorMessage = "Name Must not Exceed 100 Characters")]
		public required string Name { get; set; }

		[Required(ErrorMessage = ("Enter Your Email"))]
		[EmailAddress(ErrorMessage = "Enter Valid Email Address")]
		public required string Email { get; set; }

		[Required(ErrorMessage = ("Enter Your Password"))]
		[MinLength(8, ErrorMessage = "Password Must be at least 8 characters")]
		[MaxLength(80, ErrorMessage = "Password Must not exceed 80 Characters")]
		public required string Password { get; set; }

		[Required(ErrorMessage = ("ReEnter Your Password"))]
		[MinLength(8, ErrorMessage = "Password Must be at least 8 characters")]
		[MaxLength(80, ErrorMessage = "Password Must not exceed 80 Characters")]
		public required string ConfirmPassword { get; set; }

		[Required(ErrorMessage = "Select Either Search Jobs Or Hire")]
		public required string Role { get; set; }

		[MaxLength(100, ErrorMessage = "Company Name must not exceed 100 characters")]
		public string? CompanyName { get; set; }

	}
}
