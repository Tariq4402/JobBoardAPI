using JobBoardAPI.Entities;
using JobBoardAPI.Repositories.Interfaces;

namespace JobBoardAPI.Repositories.Interfaces
{
	public interface IEmailVerificationRepository : IGenericRepository<EmailVerificationToken>
	{
		Task<EmailVerificationToken?> GetByVerificationTokenAsync(string token);

		Task<IEnumerable<EmailVerificationToken>> GetActiveTokenByUserIdAsync(int userId);
	}
}
