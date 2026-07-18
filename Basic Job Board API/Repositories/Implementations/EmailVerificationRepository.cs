using JobBoardAPI.Entities;
using JobBoardAPI.Repositories.Interfaces;
using JobBoardAPI.Data;
using JobBoardAPI.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace JobBoardAPI.Repositories.Implementations
{
	public class EmailVerificationRepository : GenericRepository<EmailVerificationToken>, IEmailVerificationRepository
	{
		public EmailVerificationRepository(JobBoardDBContext context) : base(context) // pass context to base Generic Repository Constructor for Generic Crud Operations
		{

		}

		public async Task<EmailVerificationToken?> GetByVerificationTokenAsync(string token)
		{
			return await _context.EmailVerificationTokens.FirstOrDefaultAsync(e => e.Token == token);
		}

		public async Task<IEnumerable<EmailVerificationToken>> GetActiveTokenByUserIdAsync(int userId)
		{
			return await _context.EmailVerificationTokens.Where(e => e.UserId == userId && !e.IsUsed && e.ExpiresAt > DateTime.Now ).ToListAsync();
		}
	}
}
