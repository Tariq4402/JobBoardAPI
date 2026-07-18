using JobBoardAPI.Entities;
using JobBoardAPI.Repositories.Interfaces;
using JobBoardAPI.Data;
using JobBoardAPI.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace JobBoardAPI.Repositories.Implementations
{
	public class PasswordResetTokenRepository : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
	{
		public PasswordResetTokenRepository(JobBoardDBContext context) : base(context) // pass context to base Generic Repository Constructor for Generic Crud Operations
		{

		}

		public async Task<PasswordResetToken?> GetByTokenAsync(string token)
		{
			return await _context.PasswordResetTokens.FirstOrDefaultAsync(e => e.Token == token);
		}

		public async Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(int userId)
		{
			return await _context.PasswordResetTokens.Where(e => e.UserId == userId && !e.IsUsed && e.ExpiresAt > DateTime.Now).ToListAsync();
		}
	}
}
