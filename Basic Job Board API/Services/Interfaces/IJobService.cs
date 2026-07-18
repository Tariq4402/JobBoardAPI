using JobBoardAPI.Entities;
using JobBoardAPI.DTOs;
using JobBoardAPI.Enums;

namespace JobBoardAPI.Services.Interfaces
{
	public interface IJobService
	{
		Task<JobResponseDTO> CreateJobAsync(JobCreateDTO jobCreateDTO, int UserId);
		Task UpdateJobAsync(int JobId, JobUpdateDTO jobUpdateDTO, int UserId);
		Task DeleteJobAsync(int jobId, int UserId);
		Task<JobResponseDTO> GetJobByIdAsync(int jobId);
		Task<IEnumerable<JobResponseDTO>> GetAllJobsAsync();
		Task<PaginatedResponseDTO<JobResponseDTO>> GetFilteredJobsAsync(JobFilterDTO jobFilterDTO);
		Task UpdateJobStatusAsync(int jobId, JobStatus newStatus, int userId);
	}
}
