using JobBoardAPI.DTOs;
using FluentValidation;

namespace JobBoardAPI.Validators
{
	public class JobApplicationCreateValidator : AbstractValidator<JobApplicationCreateDTO>
	{
		public JobApplicationCreateValidator()
		{
			RuleFor(x => x.JobId)
				.GreaterThan(0)
				.WithMessage("JobId must be greater than 0.");
		}

	}
}
