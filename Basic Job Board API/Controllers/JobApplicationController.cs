using JobBoardAPI.Common;
using JobBoardAPI.DTOs;
using JobBoardAPI.Enums;
using JobBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection.Metadata.Ecma335;

namespace JobBoardAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class JobApplicationController : ControllerBase
	{
		private readonly IJobApplicationService _jobApplicationService;
		public JobApplicationController(IJobApplicationService jobApplicationService)
		{
			_jobApplicationService = jobApplicationService;
		}

		[HttpGet("job/{jobId}")]
		[Authorize (Roles = Roles.Employer)]
		public async Task<IActionResult> GetAllApplicationsForAJobAsync(int jobId)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

			var result = await _jobApplicationService.GetAllApplicationsForAJobAsync(jobId, userId);
			return Ok(result);
		}

		[HttpGet("employer")]
		[Authorize(Roles = Roles.Employer)]
		public async Task<IActionResult> GetAllJobApplicationsForEmployerAsync()
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			var result = await _jobApplicationService.GetAllJobApplicationsForEmployerAsync(userId);
			return Ok(result);
		}

		[HttpGet("Applicant")]
		[Authorize(Roles = Roles.Applicant)]
		public async Task<IActionResult> GetAllApplicationsOfApplicantAsync()
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			var result = await _jobApplicationService.GetAllApplicationsOfApplicantAsync(userId);
			return Ok(result);
		}

		[HttpDelete("{jobApplicationId}")]
		[Authorize(Roles = Roles.Applicant)]
		public async Task<IActionResult> WithdrawJobApplicationAsync(int jobApplicationId)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _jobApplicationService.WithdrawJobApplicationAsync(jobApplicationId, userId);
			
			return Ok("Application Deleted Successfully");	
		}

		[HttpGet("{jobApplicationId}")]
		[Authorize]
		public async Task<IActionResult> GetJobApplicationWithDetailsAsync(int jobApplicationId)
		{
			int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			
			var result = await _jobApplicationService.GetJobApplicationWithDetailsAsync(userId, jobApplicationId);
			return Ok(result);
		}

		[HttpPost]
		[Authorize(Roles = Roles.Applicant)]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> SubmitJobApplicationAsync(JobApplicationCreateDTO jobApplicationCreateDTO, IFormFile? file)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

			if(file != null && file.Length > 0)
			{
				// file size check
				if (file == null || file.Length == 0) return BadRequest("No File Provided");
				if (file.Length > 5 * 1024 * 1024) return BadRequest("File Size Must be less than 5MB");

				// file type check
				var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
				var extension = Path.GetExtension(file.FileName).ToLower();
				if (!allowedExtensions.Contains(extension)) return BadRequest("Only Pdf, Word or Docx Files are accepted");

				// MIME type check
				var allowedMimeTypes = new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
				if (!allowedMimeTypes.Contains(file.ContentType.ToLower())) return BadRequest("Invalid File Type");

				// Save to Disk
				var fileName = Guid.NewGuid().ToString() + extension;
				var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");
				Directory.CreateDirectory(folderPath);
				var filePath = Path.Combine(folderPath, fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				// generate Url
				var resumeUrl = $"{Request.Scheme}://{Request.Host}/uploads/resumes/{fileName}";

				//jobApplicationCreateDTO.ResumeUrl = resumeUrl;
			}
			var application = await _jobApplicationService.SubmitJobApplicationAsync(jobApplicationCreateDTO, userId);
			return CreatedAtAction(nameof(GetJobApplicationWithDetailsAsync), new { jobApplicationId = application.JobApplicationId }, application);
		}

		[HttpGet("GetApplicantProfile/{jobApplicationId}")]
		[Authorize(Roles = Roles.Employer)]
		public async Task<IActionResult> GetApplicantProfileAsync(int jobApplicationId)
		{
			var employerId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			var applicantProfile = await _jobApplicationService.GetApplicantProfileAsync(employerId, jobApplicationId);

			return Ok(applicantProfile);
		}

		[HttpPatch("{jobApplicationId}/status")]
		[Authorize(Roles = Roles.Employer)]
		public async Task<IActionResult> UpdateJobApplicationStatus(int jobApplicationId, [FromBody]UpdateApplicationStatusDTO updateApplicationStatusDTO)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _jobApplicationService.UpdateJobApplicationStatusAsync(jobApplicationId, updateApplicationStatusDTO.newStatus, userId);
			return Ok(new { Message = $"ApplicationStatus Updated to {updateApplicationStatusDTO.newStatus}" });
		}




	}
}
