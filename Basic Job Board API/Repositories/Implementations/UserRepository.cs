using JobBoardAPI.Data;
using JobBoardAPI.Entities;
using JobBoardAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobBoardAPI.Repositories.Implementations
{
	public class UserRepository : GenericRepository<User> , IUserRepository
	{
		public UserRepository(JobBoardDBContext context) : base(context)
		{
			
		}
		public async Task<IEnumerable<User>> GetByNameAsync(string name)
		{
			return await _context.Users.Where(u => u.Name.Contains(name)).ToListAsync();
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
		}

		public async Task<User?> GetByCompanyNameAsync(string comapany)
		{
			return await _context.Users.FirstOrDefaultAsync(u => u.CompanyName != null && u.CompanyName.Contains(comapany));
		}

	}
}
