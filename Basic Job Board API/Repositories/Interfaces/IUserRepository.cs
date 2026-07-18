using JobBoardAPI.Entities;

namespace JobBoardAPI.Repositories.Interfaces
{
	public interface IUserRepository : IGenericRepository<User>
	{
		Task<IEnumerable<User>> GetByNameAsync(string name);
		Task<User?> GetByEmailAsync(string email);
		Task<User?> GetByCompanyNameAsync(string company);
	}
}
