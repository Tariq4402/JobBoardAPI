using JobBoardAPI.Entities;
using JobBoardAPI.Repositories.Interfaces;

namespace JobBoardAPI.Repositories.Interfaces
{
	public interface IPasswordResetTokenRepository : IGenericRepository<PasswordResetToken>

	{
		Task<PasswordResetToken?> GetByTokenAsync(string token);
		Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(int userId);
	}
}
