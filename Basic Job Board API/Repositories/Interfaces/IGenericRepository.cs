
using System.Linq.Expressions;

namespace JobBoardAPI.Repositories.Interfaces
{
	public interface IGenericRepository<T> where T : class
	{
		Task<IEnumerable<T>> GetAllAsync();
		Task<T?> GetByIdAsync(int id);
		Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
		Task AddAsync(T entity);
		void Update(T entity);
		void Delete(T entity);
		Task SaveAsync();

	}
}
