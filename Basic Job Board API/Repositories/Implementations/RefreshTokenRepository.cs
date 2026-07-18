using JobBoardAPI.Data;
using JobBoardAPI.Entities;
using JobBoardAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace JobBoardAPI.Repositories.Implementations
{
	public class RefreshTokenRepository : GenericRepository<RefreshToken> , IRefreshTokenRepository
	{
		public RefreshTokenRepository(JobBoardDBContext context) : base(context) // pass context to base Generic Repository Constructor for Generic Crud Operations
		{

		}

		public async Task<RefreshToken?> GetByTokenAsync(string token)
		{
			return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
		}

		public async Task<IEnumerable<RefreshToken>> GetActiveTokenByUserIdAsync(int userId)
		{
			return await _context.RefreshTokens.Where(r => r.UserId == userId && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow).ToListAsync();
		}

		public async Task RevokeTokenAsync(string token)
		{
			var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
			if (refreshToken == null) return;
			refreshToken.IsRevoked = true;
		}
	}
}
