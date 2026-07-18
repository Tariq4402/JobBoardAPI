using JobBoardAPI.DTOs;
using FluentValidation;

namespace JobBoardAPI.Validators
{
	public class UserRegistrationValidator : AbstractValidator<UserRegistrationDTO>
	{
		public UserRegistrationValidator()
		{
			RuleFor(x => x.Password)
				.Matches("[A-Z]").WithMessage("Password Must Contain at least one UpperCase Letter.")
				.Matches("[0-9]").WithMessage("Password Must Contain at least one Digit.")
				.Matches("[^a-zA-Z0-9]").WithMessage("Password Must Contain at least one Special Character.")
				.Equal(x => x.ConfirmPassword).WithMessage("Password and ConfirmPassword do not match.");	

			

			RuleFor(x => x.Role)
				.Must(r => r == "Admin" || r == "Employer" || r == "Applicant")
				.WithMessage("Role Must be Admin, Employer, Applicant.");

			When(x => x.Role == "Employer", () =>
			{
				RuleFor(x => x.CompanyName)
				.NotEmpty()
				.WithMessage("Company Name is Reuired for Employers");
			});

			
		}
	}
}
