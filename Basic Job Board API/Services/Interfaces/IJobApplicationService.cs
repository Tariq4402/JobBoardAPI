using JobBoardAPI.DTOs;
using JobBoardAPI.Entities;
using JobBoardAPI.Enums;

namespace JobBoardAPI.Services.Interfaces
{
	public interface IJobApplicationService
	{
		//Task<JobApplication> GetApplicationByIdAsync(int jobId);

		// TO get job Applications for for a specific job for an Employer
		Task<IEnumerable<JobApplicationResponseDTO>> GetAllApplicationsForAJobAsync(int jobId, int userId);

		// To get all job applications for all jobs for an Employer
		Task<IEnumerable<JobApplicationResponseDTO>> GetAllJobApplicationsForEmployerAsync(int userId);

		// TO get all Job Applications of an Applicant
		Task<IEnumerable<JobApplicationResponseDTO>> GetAllApplicationsOfApplicantAsync(int userId);

		// For Applicant to withdraw their JobApplication
		Task WithdrawJobApplicationAsync(int jobApplicationId, int userId);

		// To submit the JobApplication
		Task<JobApplicationResponseDTO> SubmitJobApplicationAsync(JobApplicationCreateDTO jobApplicationCreateDTO, int userId);
		Task<JobApplicationResponseDTO> GetJobApplicationWithDetailsAsync(int userId, int jobApplicationId);

		Task<UserResponseDTO> GetApplicantProfileAsync(int employerId, int jobApplicationId);
		Task UpdateJobApplicationStatusAsync(int jobApplicationId, ApplicationStatus newStatus, int userId);

	}
}
