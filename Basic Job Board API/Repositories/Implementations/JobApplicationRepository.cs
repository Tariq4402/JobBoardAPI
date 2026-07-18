using JobBoardAPI.Data;
using JobBoardAPI.Entities;
using JobBoardAPI.Enums;
using JobBoardAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobBoardAPI.Repositories.Implementations
{
	public class JobApplicationRepository : GenericRepository<JobApplication>, IJobApplicationRepository
	{
		public JobApplicationRepository(JobBoardDBContext context) : base(context)
		{

		}

		public async Task<IEnumerable<JobApplication>> GetJobApplicationsByJobIdAsync(int JobId)
		{
			return await _context.JobApplications
				   .Include(j => j.Applicant)
				   .Include(j => j.Job)
				   .Where(j => j.JobId == JobId)
				   .ToListAsync();
		}

		public async Task<IEnumerable<JobApplication>> GetJobApplicationsByStatusAsync(ApplicationStatus status)
		{
			return await _context.JobApplications.Where(j => j.ApplicationStatus == status).ToListAsync();
		}

		public async Task<IEnumerable<JobApplication>> GetJobApplicationsByUserIdAsync(int UserId)
		{
			return await _context.JobApplications.Where(j => j.UserId == UserId).Include(j => j.Applicant)
		.Include(j => j.Job)
		.ToListAsync();
		}

		public async Task<IEnumerable<JobApplication>> GetApplicationsForEmployerAsync(int employerId)
		{
			return await _context.JobApplications
				.Include(a => a.Applicant)
				.Include(a => a.Job)
				.Where(a => a.Job!.UserId == employerId)
				.ToListAsync();
		}

		public async Task<JobApplication?> GetJobApplicationWithDetailsAsync(int jobApplicationId)
		{
			return await _context.JobApplications
	            .Include(j => j.Applicant)
	            .Include(j => j.Job)
	            .FirstOrDefaultAsync(j => j.JobApplicationId == jobApplicationId);
		}

		//public async Task<IEnumerable<JobApplication>> GetApplicationsByUserIdAsync(int userId)
		//{
		//	return await _context.JobApplications.Where(j => j.UserId == userId).ToListAsync();
		//}

	}
}
