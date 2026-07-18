using JobBoardAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
namespace JobBoardAPI.Repositories.Interfaces
{
	public interface IUnitOfWork : IDisposable
	{
		IJobApplicationRepository JobApplications { get; }
		IJobRepository Jobs { get; }
		IUserRepository Users { get; }
		IRefreshTokenRepository RefreshTokens { get; }
		IEmailVerificationRepository EmailVerificationTokens { get; }
		IPasswordResetTokenRepository PasswordResetTokens { get; }
		Task SaveAsync();

	}
}
