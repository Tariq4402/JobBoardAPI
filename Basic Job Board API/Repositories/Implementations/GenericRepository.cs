using JobBoardAPI.Data;
using JobBoardAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace JobBoardAPI.Repositories.Implementations
{
	public class GenericRepository<T> : IGenericRepository<T> where T: class
	{
		protected JobBoardDBContext _context;
		private readonly DbSet<T> _dbSet; // Represents the Table in the database
		public GenericRepository(JobBoardDBContext context)
		{
			_context = context;
			_dbSet = _context.Set<T>(); // Get the DbSet for the entity type T
		}

		public async Task<IEnumerable<T>> GetAllAsync()
		{
			return await _dbSet.AsNoTracking().ToListAsync();
		}

		public async Task<T?> GetByIdAsync(int Id)
		{
			return await _dbSet.FindAsync(Id);
		}

		public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
		{
			return await _dbSet.AnyAsync(predicate);
		}

		public async Task AddAsync(T entity)
		{
		    await _dbSet.AddAsync(entity);
		}

		public void Update(T entity)
		{
			_dbSet.Update(entity); 
		}

		public void Delete(T entity)
		{
			_dbSet.Remove(entity);
		}

		public async Task SaveAsync()
		{
			await _context.SaveChangesAsync();
		}
	}
}
