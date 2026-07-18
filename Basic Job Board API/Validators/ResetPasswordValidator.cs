using JobBoardAPI.DTOs;
using FluentValidation;

namespace JobBoardAPI.Validators
{
	public class ResetPasswordValidator : AbstractValidator<ResetPasswordDTO>
	{
		public ResetPasswordValidator()
		{
			RuleFor(x => x.Password)
				.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
			.Matches("[0-9]").WithMessage("Password must contain at least one digit.")
			.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
			.Equal(x => x.ConfirmPassword).WithMessage("Password and ConfirmPassword do not match.");
		}
	}
}
