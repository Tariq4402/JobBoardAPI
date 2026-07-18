using JobBoardAPI.Repositories.Interfaces;
using JobBoardAPI.Services.Implementations;
using JobBoardAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using JobBoardAPI.Entities;
using JobBoardAPI.DTOs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Logging;
using FluentAssertions;

namespace JobBoardAPI.Tests.Services
{
	public class AuthServiceTests
	{
		private readonly Mock<IUnitOfWork> _mockUnitOfWork;
		private readonly Mock<ITokenService> _mockTokenService;
		private readonly Mock<IEmailService> _mockEmailService;
		private readonly Mock<ILogger<AuthService>> _mockLogger;
		private readonly AuthService _authService;

		public AuthServiceTests()
		{
			_mockUnitOfWork = new Mock<IUnitOfWork>();
			_mockTokenService = new Mock<ITokenService>();
			_mockEmailService = new Mock<IEmailService>();
			_mockLogger = new Mock<ILogger<AuthService>>();
			_authService = new AuthService(
				_mockUnitOfWork.Object,
				_mockTokenService.Object,
				_mockEmailService.Object,
				_mockLogger.Object);
		}

		[Fact]
		public async Task RegisterUserAsync_ShouldRegisterUser_WhenEmailIsNotTaken()
		{
			var dto = new UserRegistrationDTO
			{
				Name = "Test User",
				Email = "test@example.com",
				Password = "Password123!",
				ConfirmPassword = "Password123!",
				Role = "Applicant"
			};
			_mockUnitOfWork.Setup(u => u.Users.GetByEmailAsync(dto.Email))
				.ReturnsAsync((User?)null);
			_mockUnitOfWork.Setup(u => u.Users.AddAsync(It.IsAny<User>()))
				.Returns(Task.CompletedTask);
			_mockUnitOfWork.Setup(u => u.SaveAsync())
				.Returns(Task.CompletedTask);
			_mockTokenService.Setup(t => t.GenerateSecureToken())
				.Returns("mock-token");
			_mockUnitOfWork.Setup(u => u.EmailVerificationTokens.AddAsync(It.IsAny<EmailVerificationToken>()))
				.Returns(Task.CompletedTask);
			_mockEmailService.Setup(e => e.SendEmailVerificationAsync(dto.Email, It.IsAny<string>()))
				.Returns(Task.CompletedTask);

			await _authService.RegisterUserAsync(dto);

			// Assert
			_mockUnitOfWork.Verify(u => u.Users.AddAsync(It.IsAny<User>()), Times.Once);
			_mockUnitOfWork.Verify(u => u.EmailVerificationTokens.AddAsync(It.IsAny<EmailVerificationToken>()), Times.Once);
			_mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Exactly(2));
			_mockEmailService.Verify(e => e.SendEmailVerificationAsync(dto.Email, It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async Task RegisterUserAsync_ShouldThrowInvalidOperationException_WhenEmailIsAlreadyTaken()
		{
			//Arrange
			var dto = new UserRegistrationDTO
			{
				Name = "Test User",
				Email = "taken@example.com",
				Password = "Password123!",
				ConfirmPassword = "Password123!",
				Role = "Applicant"
			};

			_mockUnitOfWork.Setup(u => u.Users.GetByEmailAsync(dto.Email))
				.ReturnsAsync(new User
				{
					Name = "Existing User",
					Email = dto.Email,
					PasswordHash = "some-hash",
					Role = "Applicant"
				});

			// Act
			Func<Task> act = async() => await _authService.RegisterUserAsync(dto);

			//Assert
			await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("This Email is ALready Registered.");
		}

		[Fact]
		public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
		{
			// Arrange
			var dto = new UserLoginDTO
			{
				Email = "test@example.com",
				Password = "Password123!"
			};

			var existingUser = new User
			{
				UserId = 1,
				Name = "Test",
				Email = dto.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
				IsEmailVerified = true,
				Role = "Applicant"
			};

			_mockUnitOfWork.Setup(u => u.Users.GetByEmailAsync(dto.Email))
				.ReturnsAsync(existingUser);
			_mockTokenService.Setup(t => t.GenerateToken(existingUser))
				.Returns("mock-jwt-Token");
			_mockTokenService.Setup(t => t.GenerateRefreshToken(existingUser.UserId))
				.Returns(new RefreshToken
				{
					Token = "mock-refresh-token",
					UserId = existingUser.UserId,
					ExpiresAt = DateTime.Now.AddDays(7),
					IsRevoked = false
				});
			_mockUnitOfWork.Setup(u => u.RefreshTokens.AddAsync(It.IsAny<RefreshToken>()))
				.Returns(Task.CompletedTask);
			_mockUnitOfWork.Setup(u => u.SaveAsync())
				.Returns(Task.CompletedTask);

			//Act
			var result = await _authService.LoginAsync(dto);

			//Assert
			result.Should().NotBeNull();
			result.AccessToken.Should().Be("mock-jwt-Token");
			result.RefreshToken.Should().Be("Mock-refresh-token");
		}

		[Fact]
		public async Task LoginAsync_ShouldThrowException_WhenUserDoesNotExist()
		{
			//Arrange
			var dto = new UserLoginDTO
			{
				Email = "test@example.com",
				Password = "Password123!"
			};
			_mockUnitOfWork.Setup(u => u.Users.GetByEmailAsync(dto.Email))
				.ReturnsAsync((User?)null);

			//Act
			Func<Task> act = async() => await _authService.LoginAsync(dto);

			//Assert
			await act.Should().ThrowAsync<UnauthorizedAccessException>()
				.WithMessage("Invalid email or Password.");
		}

		[Fact]
		public async Task LoginAsync_ShouldThrowException_WhenPasswordIsInvalid()
		{
			// Arrange
			var dto = new UserLoginDTO
			{
				Email = "test@example.com",
				Password = "Password123!"
			};

			var existingUser = new User
			{
				UserId = 1,
				Name = "Test",
				Email = "test@example.com",
				PasswordHash = BCrypt.Net.BCrypt.HashPassword("Wrong@Password"),
				Role = "Applicant"
			};

			_mockUnitOfWork.Setup(u => u.Users.GetByEmailAsync(dto.Email))
				.ReturnsAsync(existingUser);

			// Act
			Func<Task> act = async() => await _authService.LoginAsync(dto);

			// Assert
			await act.Should().ThrowAsync<UnauthorizedAccessException>()
				.WithMessage("Invalid email or Password.");
		}

		[Fact]
		public async Task LoginAsync_ShouldThrowException_WhenEmailIsNotVerified()
		{
			// Arrange
			var dto = new UserLoginDTO
			{
				Email = "test@example.com",
				Password = "Password123!"
			};

			var existingUser = new User
			{
				UserId = 1,
				Name = "Test",
				Email = "test@example.com",
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
				Role = "Applicant",
				IsEmailVerified = false
			};

			_mockUnitOfWork.Setup(u => u.Users.GetByEmailAsync(dto.Email))
				.ReturnsAsync(existingUser);

			// Act
			Func<Task> act = async () => await _authService.LoginAsync(dto);

			// Assert
			await act.Should().ThrowAsync<UnauthorizedAccessException>()
				.WithMessage("Please Verify you Email First");
		}

		[Fact]
		public async Task LoginAsync_ShouldThrowException_WhenAccountIsBlocked()
		{
			// Arrange
			var dto = new UserLoginDTO
			{
				Email = "test@example.com",
				Password = "Password123!"
			};
			var existinUser = new User
			{
				UserId = 1,
				Name = "Test",
				Email = "test@example.com",
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
				Role = "Applicant",
				IsBlocked = true,
				IsEmailVerified = true
			};

			_mockUnitOfWork.Setup(u => u.Users.GetByEmailAsync(dto.Email))
				.ReturnsAsync(existinUser);

			// Act
			Func<Task> act = async() => await _authService.LoginAsync(dto);

			// Assert
			await act.Should().ThrowAsync<UnauthorizedAccessException>()
				.WithMessage("Your Account is Blocked.");
		}

	}
}
