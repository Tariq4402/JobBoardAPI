using JobBoardAPI.Entities;
using JobBoardAPI.Enums;

namespace JobBoardAPI.Repositories.Interfaces
{
	public interface IJobApplicationRepository : IGenericRepository<JobApplication>
	{
		Task<IEnumerable<JobApplication>> GetJobApplicationsByJobIdAsync(int jobId);
		Task<IEnumerable<JobApplication>> GetJobApplicationsByStatusAsync(ApplicationStatus status);
		Task<IEnumerable<JobApplication>> GetJobApplicationsByUserIdAsync(int userId);
		Task<IEnumerable<JobApplication>> GetApplicationsForEmployerAsync(int employerId);
		Task<JobApplication?> GetJobApplicationWithDetailsAsync(int jobApplicationId);
		//Task<IEnumerable<JobApplication>> GetApplicationsByUserIdAsync(int userId);

	}
}
