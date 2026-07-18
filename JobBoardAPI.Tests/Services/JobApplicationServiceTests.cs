using AutoMapper;
using FluentAssertions;
using JobBoardAPI.DTOs;
using JobBoardAPI.Entities;
using JobBoardAPI.Enums;
using JobBoardAPI.Repositories.Interfaces;
using JobBoardAPI.Services.Implementations;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace JobBoardAPI.Tests.Services
{
	public class JobApplicationServiceTests
	{
		private readonly Mock<IUnitOfWork> _mockUnitOfWork;
		private readonly Mock<IMapper> _mockMapper;
		private readonly JobApplicationService _jobApplicationService;

		public JobApplicationServiceTests()
		{
			_mockUnitOfWork = new Mock<IUnitOfWork>();
			_mockMapper = new Mock<IMapper>();
			_jobApplicationService = new JobApplicationService(
				_mockUnitOfWork.Object,
				_mockMapper.Object);
		}

		[Fact]
		public async Task SubmitJobApplicatiopnAsync_ShouldSubmitApplication_WhenValid()
		{
			// Arrange
			var dto = new JobApplicationCreateDTO
			{
				
				JobId = 1,
				CoverLetter = "I have the perfect skillset for this Role."
			};
			var job = new Job
			{
				JobTitle = "Test",
				JobDescription = "Test Description",
				JobLocation = "Remote",
				JobStatus = JobStatus.Open
			};
			var application = new JobApplication
			{
				JobApplicationId = 1,
				JobId = dto.JobId,
				UserId = 1,
				AppliedDate = DateTime.Now,
				CoverLetter = dto.CoverLetter
			};
			var applicationResponsedto = new JobApplicationResponseDTO
			{
				ApplicantName = "Test",
				JobTitle = "Test Title",
				CoverLetter = dto.CoverLetter,
				AppliedDate = application.AppliedDate,
			};
			var userId = 1;

			_mockUnitOfWork.Setup(j => j.Jobs.GetByIdAsync(dto.JobId))
				.ReturnsAsync(job);
			_mockUnitOfWork.Setup(j => j.JobApplications.ExistsAsync(It.IsAny<Expression<Func<JobApplication, bool>>>()))
				.ReturnsAsync(false);
			_mockMapper.Setup(a => a.Map<JobApplication>(dto))
				.Returns(application);

			_mockUnitOfWork.Setup(a => a.JobApplications.AddAsync(application))
				.Returns(Task.CompletedTask);
			_mockUnitOfWork.Setup(a => a.SaveAsync())
				.Returns(Task.CompletedTask);
			_mockUnitOfWork.Setup(a => a.JobApplications.GetJobApplicationWithDetailsAsync(application.JobApplicationId))
				.ReturnsAsync(application);
			_mockMapper.Setup(m => m.Map<JobApplicationResponseDTO>(application))
				.Returns(applicationResponsedto);

			// Act
			var result = await _jobApplicationService.SubmitJobApplicationAsync(dto, userId);

			// Assert
			_mockUnitOfWork.Verify(a => a.JobApplications.AddAsync(It.IsAny<JobApplication>()), Times.Once);
			_mockUnitOfWork.Verify(a => a.SaveAsync(), Times.Once);
			result.Should().NotBeNull();
			result.CoverLetter.Should().Be(dto.CoverLetter);
			_mockUnitOfWork.Verify(a => a.JobApplications.GetJobApplicationWithDetailsAsync(application.JobApplicationId), Times.Once);

		}

		[Fact]
		public async Task SubmitJobApplicationAsync_ShouldThrow_WhenJobIsClosed()
		{
			// Arrange
			var dto = new JobApplicationCreateDTO
			{

				JobId = 1,
				CoverLetter = "I have the perfect skillset for this Role."
			};
			var job = new Job
			{
				JobTitle = "Test",
				JobDescription = "Test Description",
				JobLocation = "Remote",
				JobStatus = JobStatus.Closed
			};
			
			var userId = 1;

			_mockUnitOfWork.Setup(j => j.Jobs.GetByIdAsync(dto.JobId))
				.ReturnsAsync(job);
			_mockUnitOfWork.Setup(j => j.JobApplications.ExistsAsync(It.IsAny<Expression<Func<JobApplication, bool>>>()))
				.ReturnsAsync(false);
			

			// Act
			Func<Task> act = async() => await _jobApplicationService.SubmitJobApplicationAsync(dto, userId);

			// Assert
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("This Employer is no longer Accepting Applications");

		}

		[Fact]
		public async Task GetJobApplicationWithDetailsAsync_ShouldThrow_WhenUserIsNeitherApplicantNorEmployer()
		{
			// Setup
			var application = new JobApplication
			{
				JobApplicationId = 1,
				UserId = 2, // Applicant UserId
				Job = new Job
				{
					UserId = 3, // Employer UserId
					JobTitle = "Test JobTitle",
					JobDescription = "Test Description",
					JobLocation = "Remote"
				}
			};
			var userId = 71;
			_mockUnitOfWork.Setup(j => j.JobApplications.GetJobApplicationWithDetailsAsync(application.JobApplicationId))
				.ReturnsAsync(application);

			// Act
			Func<Task> act = async() => await _jobApplicationService.GetJobApplicationWithDetailsAsync(userId, application.JobApplicationId);

			// Assert
			await act.Should().ThrowAsync<UnauthorizedAccessException>();

		}
	}
}
