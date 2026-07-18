using JobBoardAPI.DTOs;
using FluentValidation;

namespace JobBoardAPI.Validators
{
	public class ChangePasswordValidator : AbstractValidator<ChangePasswordDTO>
	{
		public ChangePasswordValidator()
		{
			RuleFor(x => x.NewPassword)
				.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
			.Matches("[0-9]").WithMessage("Password must contain at least one digit.")
			.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
			.Equal(x => x.ConfirmPassword).WithMessage("New Password and Confirm Password do not match.");
		}
	}
}
