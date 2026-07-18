using JobBoardAPI.Entities;

namespace JobBoardAPI.Repositories.Interfaces
{
	public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
	{
		Task<RefreshToken?> GetByTokenAsync(string token);
		Task<IEnumerable<RefreshToken>> GetActiveTokenByUserIdAsync(int userId);
		Task RevokeTokenAsync(string token);
	}
}
