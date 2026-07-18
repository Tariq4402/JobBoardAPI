using AutoMapper;
using JobBoardAPI.DTOs;
using JobBoardAPI.Entities;
using JobBoardAPI.Enums;
using JobBoardAPI.Repositories.Interfaces;
using JobBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobBoardAPI.Services.Implementations
{
	public class JobApplicationService : IJobApplicationService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		public JobApplicationService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		//public async Task<IEnumerable<JobApplication>> GetJobByIdAsync(int jobId)
		//{ 
		//	return await _unitOfWork.JobApplications.GetJobApplicationsByJobIdAsync(jobId);
		//}

		public async Task<IEnumerable<JobApplicationResponseDTO>> GetAllApplicationsForAJobAsync(int jobId, int userId)
		{
			var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
			if (job == null) throw new KeyNotFoundException("No Job Exists");
			if(job.UserId != userId) throw new UnauthorizedAccessException("You can See Applications for your own Job");

			var jobApplications = await _unitOfWork.JobApplications.GetJobApplicationsByJobIdAsync(jobId);

			return _mapper.Map<IEnumerable<JobApplicationResponseDTO>>(jobApplications);

		}

		public async Task<IEnumerable<JobApplicationResponseDTO>> GetAllJobApplicationsForEmployerAsync(int userId)
		{
			var jobApplications = await _unitOfWork.JobApplications.GetApplicationsForEmployerAsync(userId);
			if (!jobApplications.Any()) throw new KeyNotFoundException("No Applications Exist"); 
			return _mapper.Map<IEnumerable<JobApplicationResponseDTO>>(jobApplications);
		}

		public async Task<IEnumerable<JobApplicationResponseDTO>> GetAllApplicationsOfApplicantAsync(int userId)
		{
			var jobApplications = await _unitOfWork.JobApplications.GetJobApplicationsByUserIdAsync(userId);
			if (!jobApplications.Any()) throw new KeyNotFoundException("No Job Applications Yet");
			return _mapper.Map<IEnumerable<JobApplicationResponseDTO>>(jobApplications);
		}

		public async Task WithdrawJobApplicationAsync(int jobApplicationId, int userId)
		{
			var jobApplication = await _unitOfWork.JobApplications.GetByIdAsync(jobApplicationId);
			if (jobApplication == null) throw new KeyNotFoundException();

			if (jobApplication.UserId != userId) throw new UnauthorizedAccessException();

			_unitOfWork.JobApplications.Delete(jobApplication);
			await _unitOfWork.SaveAsync();
		}

		public async Task<JobApplicationResponseDTO> SubmitJobApplicationAsync(JobApplicationCreateDTO jobApplicationCreateDTO, int userId)
		{
			var job = await _unitOfWork.Jobs.GetByIdAsync(jobApplicationCreateDTO.JobId);
			if (job == null) throw new KeyNotFoundException("Job does not exist");

			if (job.JobStatus != JobStatus.Open) throw new InvalidOperationException("This Employer is no longer Accepting Applications");

			bool isAlreadyApplied = await _unitOfWork.JobApplications.
				ExistsAsync(j => j.JobId == jobApplicationCreateDTO.JobId && j.UserId ==userId);
			if (isAlreadyApplied) throw new InvalidOperationException("You have Already Applied to this Job");

			var jobApplication = _mapper.Map<JobApplication>(jobApplicationCreateDTO);
			jobApplication.AppliedDate = DateTime.Now;

			jobApplication.UserId = userId;
			await _unitOfWork.JobApplications.AddAsync(jobApplication);
			await _unitOfWork.SaveAsync();
			var saved = await _unitOfWork.JobApplications.GetJobApplicationWithDetailsAsync(jobApplication.JobApplicationId);
			return _mapper.Map<JobApplicationResponseDTO>(saved);
		}

		public async Task<JobApplicationResponseDTO> GetJobApplicationWithDetailsAsync(int userId, int jobApplicationId)
		{

			var result = await _unitOfWork.JobApplications.GetJobApplicationWithDetailsAsync(jobApplicationId);
			if (result == null) throw new KeyNotFoundException("Job Application Not Found.");
			if (result.Job == null) throw new KeyNotFoundException();
			bool isApplicant = result.UserId == userId;
			bool isEmployer = result.Job.UserId == userId;
			if (!isApplicant && !isEmployer) throw new UnauthorizedAccessException();
			return _mapper.Map<JobApplicationResponseDTO>(result);
		}


		public async Task<UserResponseDTO> GetApplicantProfileAsync(int employerId, int jobApplicationId)
		{
			var application = await _unitOfWork.JobApplications.GetJobApplicationWithDetailsAsync(jobApplicationId);
			if (application == null) throw new KeyNotFoundException("No ApplicationFound");
			if (application.Job!.UserId != employerId) throw new UnauthorizedAccessException();
			var result = await _unitOfWork.Users.GetByIdAsync(application.UserId);
			if (result == null) throw new KeyNotFoundException("Applicant not found");

			return _mapper.Map<UserResponseDTO>(result);
		}

		public async Task UpdateJobApplicationStatusAsync(int jobApplicationId, ApplicationStatus newStatus, int userId)
		{
			var application = await _unitOfWork.JobApplications.GetJobApplicationWithDetailsAsync(jobApplicationId);
			if (application == null) throw new KeyNotFoundException("Application Not Found.");
			if (application.Job == null) throw new InvalidOperationException("This Application is not associated to any Job."); 
			if(application.Job.UserId != userId) throw new UnauthorizedAccessException();
			if (application.ApplicationStatus == newStatus) throw new InvalidOperationException("This Application already have this status.");
			application.ApplicationStatus = newStatus;
			await _unitOfWork.SaveAsync();
		}

		


	}
}
