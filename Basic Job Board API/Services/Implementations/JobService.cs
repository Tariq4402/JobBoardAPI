using JobBoardAPI.DTOs;
using JobBoardAPI.Repositories.Interfaces;
using JobBoardAPI.Services.Interfaces;
using JobBoardAPI.Entities;
using JobBoardAPI.Enums;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;

namespace JobBoardAPI.Services.Implementations
{
	public class JobService : IJobService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IMemoryCache _memoryCache;
		public JobService(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache memoryCache)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_memoryCache = memoryCache;
		}
		public async Task<JobResponseDTO> CreateJobAsync(JobCreateDTO jobCreateDto, int UserId)
		{
			
			var job = _mapper.Map<Job>(jobCreateDto);
			job.PostedDate = DateTime.UtcNow;
			job.JobStatus = JobStatus.Open;
			job.UserId = UserId;
			bool isDuplicate = await _unitOfWork.Jobs.ExistsAsync(j =>
	             j.UserId == UserId &&
	             j.JobTitle == jobCreateDto.JobTitle &&
	             j.JobLocation == jobCreateDto.JobLocation &&
				 j.JobType == jobCreateDto.JobType);

			if (isDuplicate) throw new InvalidOperationException("You have already posted this job.");
			await _unitOfWork.Jobs.AddAsync(job);
			await _unitOfWork.SaveAsync();

			var CreatedJob = await _unitOfWork.Jobs.GetJobWithEmployerAsync(job.JobId);
			return _mapper.Map<JobResponseDTO>(CreatedJob);
		}

		public async Task UpdateJobAsync(int JobId, JobUpdateDTO jobUpdateDTO, int UserId)
		{
			var job = await _unitOfWork.Jobs.GetByIdAsync(JobId);
			if (job == null) throw new KeyNotFoundException();
			if (job.UserId != UserId) throw new UnauthorizedAccessException();

			if(jobUpdateDTO.JobTitle != null) job.JobTitle = jobUpdateDTO.JobTitle;
			if(jobUpdateDTO.JobDescription != null) job.JobDescription = jobUpdateDTO.JobDescription;
			if(jobUpdateDTO.JobType != null) job.JobType = jobUpdateDTO.JobType.Value;
			if(jobUpdateDTO.JobStatus != null) job.JobStatus = jobUpdateDTO.JobStatus.Value;
			if(jobUpdateDTO.JobLocation != null) job.JobLocation = jobUpdateDTO.JobLocation;
			if(jobUpdateDTO.MinSalary != null) job.MinSalary = jobUpdateDTO.MinSalary.Value;
			if(jobUpdateDTO.MaxSalary != null) job.MaxSalary = jobUpdateDTO.MaxSalary.Value;

			_unitOfWork.Jobs.Update(job);
			await _unitOfWork.SaveAsync();
			
		}

		public async Task DeleteJobAsync(int JobId, int UserId)
		{
			var job = await _unitOfWork.Jobs.GetByIdAsync(JobId);
			if(job == null) throw new KeyNotFoundException();
			if (job.UserId != UserId) throw new UnauthorizedAccessException();
			_unitOfWork.Jobs.Delete(job);
			await _unitOfWork.SaveAsync();
		}

		public async Task<JobResponseDTO> GetJobByIdAsync(int JobId)
		{
			var job = await _unitOfWork.Jobs.GetJobWithEmployerAsync(JobId);
			if(job == null) throw new KeyNotFoundException("Job does not exist");
			return _mapper.Map<JobResponseDTO>(job);
		}

		public async Task<IEnumerable<JobResponseDTO>> GetAllJobsAsync()
		{
			var jobs = await _unitOfWork.Jobs.GetAllJobsWithEmployerAsync();
			return _mapper.Map<IEnumerable<JobResponseDTO>>(jobs);

		}

		public async Task<PaginatedResponseDTO<JobResponseDTO>> GetFilteredJobsAsync(JobFilterDTO jobFilterDTO)
		{
			// Build a unique cache key based on all filter parameters so different filter combinations are cached separately
			var cacheKey = $"jobs_p{jobFilterDTO.Page}_ps{jobFilterDTO.PageSize}_t{jobFilterDTO.JobTitle}_l{jobFilterDTO.Location}_jt{jobFilterDTO.JobType}_js{jobFilterDTO.JobStatus}_min{jobFilterDTO.MinSalary}_max{jobFilterDTO.MaxSalary}";

			// Check if result for these exact filters already exists in cache,if exist return it without hitting DB
			if (_memoryCache.TryGetValue(cacheKey, out PaginatedResponseDTO<JobResponseDTO>? cachedResult) && cachedResult != null)
				return cachedResult; // Returning cached Jobs

			// Cache miss — fetch from DB and map to response DTO
			var (jobs, totalCount) = await _unitOfWork.Jobs.GetFilteredJobsAsync(jobFilterDTO);

			var result = new PaginatedResponseDTO<JobResponseDTO>
			{
				Data = _mapper.Map<IEnumerable<JobResponseDTO>>(jobs),
				TotalCount = totalCount,
				Page = jobFilterDTO.Page,
				PageSize = jobFilterDTO.PageSize,
				TotalPages = (int)Math.Ceiling((double)totalCount / jobFilterDTO.PageSize)

			};

			// store result in cache for 5 mins, after that it will expire and fresh DB queuery should be made
			_memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
			return result;
		}

		public async Task UpdateJobStatusAsync(int jobId, JobStatus newStatus, int userId)
		{
			var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
			if (job == null) throw new KeyNotFoundException("Job Not Found");
			if (job.UserId != userId) throw new UnauthorizedAccessException();
			if (job.JobStatus == newStatus) throw new InvalidOperationException("This Job Already have this Status.");
			job.JobStatus = newStatus;
			await _unitOfWork.SaveAsync();
		}
	}
}
