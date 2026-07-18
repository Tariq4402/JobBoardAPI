using JobBoardAPI.Repositories.Implementations;
using JobBoardAPI.Repositories.Interfaces;
using JobBoardAPI.Data;
using JobBoardAPI.Migrations;

namespace JobBoardAPI.Repositories.Implementations
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly JobBoardDBContext _context; // class feild to hold the database context

		// Repository Properties exposing data access interfaces
		public IJobApplicationRepository JobApplications { get; }
		public IJobRepository Jobs { get; }
		public IUserRepository Users { get; }
		public IRefreshTokenRepository RefreshTokens { get; }
		public IEmailVerificationRepository EmailVerificationTokens { get; }
		public IPasswordResetTokenRepository PasswordResetTokens { get; }


		public UnitOfWork(JobBoardDBContext context) // constructor parameter gets the DbContext injected
		{
			_context = context;
			JobApplications = new JobApplicationRepository(context);
			Jobs = new JobRepository(context);
			Users = new UserRepository(context);
			RefreshTokens = new RefreshTokenRepository(context);
			EmailVerificationTokens = new EmailVerificationRepository(context);
			PasswordResetTokens = new PasswordResetTokenRepository(context);
		}

		public async Task SaveAsync()
		{
			 await _context.SaveChangesAsync();
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
