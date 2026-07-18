using JobBoardAPI.Data;
using JobBoardAPI.DTOs;
using JobBoardAPI.Entities;
using JobBoardAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobBoardAPI.Repositories.Implementations
{
	public class JobRepository : GenericRepository<Job> , IJobRepository
	{
		//private readonly JobBoardDBContext _context;
		public JobRepository(JobBoardDBContext context) : base(context) // pass context to base Generic Repository Constructor for Generic Crud Operations
		{

		}
		public async Task<IEnumerable<Job>> GetByTitleAsync(string JobTitle)
		{
			return await _context.Jobs.Where(J => J.JobTitle.Contains(JobTitle)).ToListAsync();

		}

		public async Task<Job?> GetJobWithEmployerAsync(int jobId)
		{
			return await _context.Jobs.Include(j => j.Employer).FirstOrDefaultAsync(j => j.JobId == jobId);
		}

		public async Task<IEnumerable<Job?>> GetAllJobsWithEmployerAsync()
		{
			return await _context.Jobs.Include(j => j.Employer).ToListAsync();
		}

		public async Task<IEnumerable<Job>> GetJobsByIdAsync(int userId)
		{
			return await _context.Jobs.Where(j => j.UserId == userId).ToListAsync();
		}

		public async Task<(IEnumerable<Job> Jobs, int TotalCount)> GetFilteredJobsAsync(JobFilterDTO jobFilterDTO)
		{
			var query = _context.Jobs.Include(j => j.Employer).AsQueryable();

			if (!string.IsNullOrEmpty(jobFilterDTO.JobTitle))
				query = query.Where(j => j.JobTitle.Contains(jobFilterDTO.JobTitle));

			if (!string.IsNullOrEmpty(jobFilterDTO.Location))
				query = query.Where(j => j.JobLocation.Contains(jobFilterDTO.Location));

			if (jobFilterDTO.MinSalary.HasValue)
				query = query.Where(j => j.MinSalary >= jobFilterDTO.MinSalary.Value);
			if (jobFilterDTO.MaxSalary.HasValue)
				query = query.Where(j => j.MaxSalary <= jobFilterDTO.MaxSalary.Value);

			if(jobFilterDTO.JobType.HasValue)
				query = query.Where(j => j.JobType == jobFilterDTO.JobType.Value);

			if (jobFilterDTO.JobStatus.HasValue)
				query = query.Where(j => j.JobStatus == jobFilterDTO.JobStatus.Value);

			var totalCount = await query.CountAsync();

			var jobs = await query
				.Skip((jobFilterDTO.Page - 1) * jobFilterDTO.PageSize)
				.Take(jobFilterDTO.PageSize)
				.ToListAsync();

			return (jobs,  totalCount);
		}
	}
}
