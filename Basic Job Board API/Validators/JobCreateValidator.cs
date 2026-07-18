using FluentValidation;
using JobBoardAPI.DTOs;

namespace JobBoardAPI.Validators
{
	public class JobCreateValidator : AbstractValidator<JobCreateDTO>
	{
		public JobCreateValidator()
		{
			RuleFor(x => x.MinSalary)
				.LessThanOrEqualTo(x => x.MaxSalary)
				.WithMessage("Minimum Salary Must be Less than or Equal to MaxSalary");


		}
	}
}
