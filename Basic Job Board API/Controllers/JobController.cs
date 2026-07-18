using JobBoardAPI.DTOs;
using JobBoardAPI.Enums;
using JobBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobBoardAPI.Common;
using Microsoft.AspNetCore.RateLimiting;

namespace JobBoardAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class JobController : ControllerBase
	{
		private readonly IJobService _jobService;
		public JobController(IJobService jobService )
		{
			_jobService = jobService;
		}


		[HttpGet("{jobId}")]
		public async Task<IActionResult> GetJobByIdAsync(int jobId)
		{
			var result = await _jobService.GetJobByIdAsync(jobId);
			//if (result == null) return NotFound();
			return Ok(result);
		}


		[HttpPost]
		[Authorize(Roles = Roles.Employer)]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> CreateJobAsync([FromBody]JobCreateDTO jobCreateDTO)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			//if (jobCreateDTO == null) return BadRequest("Please Fill The Required feids");
			var createdJob = await _jobService.CreateJobAsync(jobCreateDTO, userId);
			return CreatedAtAction(nameof(GetJobByIdAsync), new { jobId = createdJob.JobId }, createdJob);
		}

		[HttpPut("{JobId}")]
		[Authorize(Roles = Roles.Employer)]
		public async Task<IActionResult> UpdateJobAsync(int JobId, [FromBody]JobUpdateDTO jobUpdateDTO)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _jobService.UpdateJobAsync(JobId, jobUpdateDTO, userId);
			
			return Ok("Job Updated Successfully");
		}

		[HttpDelete("{jobId}")]
		[Authorize(Roles = Roles.Employer)]
		public async Task<IActionResult> DeleteJobAsync(int jobId)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _jobService.DeleteJobAsync(jobId, userId);
			
			return Ok("Job Deleted Successfully");
		}

		

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAllJobsAsync()
		{
			var result = await _jobService.GetAllJobsAsync();
			return Ok(result);
		}

		[HttpGet("filter")]
		[Authorize]
		public async Task<IActionResult> GetFilteredJobsAsync([FromQuery]JobFilterDTO jobFilterDTO)
		{
			var result = await _jobService.GetFilteredJobsAsync(jobFilterDTO);
			return Ok(result);
		}

		[HttpPatch("{jobId}/status")]
		[Authorize(Roles = Roles.Employer)]
		public async Task<IActionResult> UpdateJobStatusAsync(int jobId, [FromBody]UpdateJobStatusDTO updateJobStatusDTO)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _jobService.UpdateJobStatusAsync(jobId, updateJobStatusDTO.newStatus, userId);
			return Ok(new { Message = $"JobStatus Updated Successfully to {updateJobStatusDTO.newStatus}." });
		}


	}
}
