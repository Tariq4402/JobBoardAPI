using JobBoardAPI.DTOs;
using FluentValidation;

namespace JobBoardAPI.Validators
{
	public class JobUpdateValidator : AbstractValidator<JobUpdateDTO>
	{
		public JobUpdateValidator()
		{
			When(x => x.MinSalary.HasValue && x.MaxSalary.HasValue, () =>
			{
				RuleFor(x => x.MinSalary)
				.LessThanOrEqualTo(x => x.MaxSalary).
				WithMessage("Minimum Salary must be Less than Maximum Salary.");
			});
		}
	}
}
