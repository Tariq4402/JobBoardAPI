using JobBoardAPI.DTOs;
using JobBoardAPI.Entities;

namespace JobBoardAPI.Repositories.Interfaces
{
	public interface IJobRepository : IGenericRepository<Job>
	{
		Task<IEnumerable<Job>> GetByTitleAsync(string JobTitle);

		Task<Job?> GetJobWithEmployerAsync(int jobId);

		Task<IEnumerable<Job?>> GetAllJobsWithEmployerAsync();

		Task<IEnumerable<Job>> GetJobsByIdAsync(int userId);

		Task<(IEnumerable<Job> Jobs, int TotalCount)> GetFilteredJobsAsync(JobFilterDTO jobFilterDTO);

	}
}
