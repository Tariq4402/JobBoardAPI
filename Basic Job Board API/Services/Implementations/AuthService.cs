using JobBoardAPI.DTOs;
using JobBoardAPI.Entities;
using JobBoardAPI.Services.Interfaces;
using JobBoardAPI.Repositories.Interfaces;
namespace JobBoardAPI.Services.Implementations
{
	public class AuthService : IAuthService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ITokenService _tokenService;
		private readonly IEmailService _emailService;
		private readonly ILogger<AuthService> _logger;
		public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IEmailService emailService, ILogger<AuthService> logger)
		{
			_unitOfWork = unitOfWork;
			_tokenService = tokenService;
			_emailService = emailService;
			_logger = logger;
		}

		public async Task RegisterUserAsync(UserRegistrationDTO userRegistrationDTO)
		{
			if (userRegistrationDTO == null) return;

			//else if (userRegistrationDTO.Password != userRegistrationDTO.ConfirmPassword) return false;

			var existingUser = await _unitOfWork.Users.GetByEmailAsync(userRegistrationDTO.Email);
			if (existingUser != null) throw new InvalidOperationException("This Email is already Registered");

				var user = new User()
				{
					Name = userRegistrationDTO.Name,
					Email = userRegistrationDTO.Email,
					PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegistrationDTO.Password),
					Role = userRegistrationDTO.Role,
					CompanyName = userRegistrationDTO.CompanyName

				};
				 await _unitOfWork.Users.AddAsync(user);
			     await _unitOfWork.SaveAsync();
			var secureToken = _tokenService.GenerateSecureToken();
			var emailVerificationToken = new EmailVerificationToken()
			{
				Token = secureToken,
				ExpiresAt = DateTime.Now.AddMinutes(60),
				IsUsed = false,
				UserId = user.UserId
			};
			await _unitOfWork.EmailVerificationTokens.AddAsync(emailVerificationToken);
			await _unitOfWork.SaveAsync();
			await _emailService.SendEmailVerificationAsync(user.Email, secureToken);
			_logger.LogInformation("Verification email sent to {Email}", user.Email);


		}

		public async Task<LoginResponseDTO> LoginAsync(UserLoginDTO userLoginDTO)
		{
			var user = await _unitOfWork.Users.GetByEmailAsync(userLoginDTO.Email);
			if (user == null) throw new UnauthorizedAccessException("Invalid email or Password.");

			bool isPasswordValid = BCrypt.Net.BCrypt.Verify(userLoginDTO.Password, user.PasswordHash);
			if (!isPasswordValid) throw new UnauthorizedAccessException("Invalid email or Password.");

			if (!user.IsEmailVerified) throw new UnauthorizedAccessException("Please Verify you Email First");
			if (user.IsBlocked) throw new UnauthorizedAccessException("Your Account is Blocked.");


			var token = _tokenService.GenerateToken(user);

			var refreshToken = _tokenService.GenerateRefreshToken(user.UserId);
			await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
			await _unitOfWork.SaveAsync();

			return new LoginResponseDTO
			{
				AccessToken = token,
				RefreshToken = refreshToken.Token
			};

		}

		public async Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken)
		{
			var RefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
			if (RefreshToken == null) throw new KeyNotFoundException("Refresh Token Not Found");
			var user = await _unitOfWork.Users.GetByIdAsync(RefreshToken.UserId);
			if(user == null) throw new KeyNotFoundException("No user Found having this Refresh Token");
			//var activeToken = await _unitOfWork.RefreshTokens.GetActiveTokenByUserIdAsync(user.UserId);
			if (RefreshToken.IsRevoked || RefreshToken.ExpiresAt < DateTime.Now) throw new InvalidOperationException("Invalid Refresh Token, Login Again.");

			
			await _unitOfWork.RefreshTokens.RevokeTokenAsync(RefreshToken.Token); // Revoking the old refresh token
			var refreshtoken = _tokenService.GenerateRefreshToken(user.UserId); // Generate New Refresh Token
			await _unitOfWork.RefreshTokens.AddAsync(refreshtoken); // Adding new Refresh Token to DB
			await _unitOfWork.SaveAsync(); // Saving Changes to Db
			var accessToken = _tokenService.GenerateToken(user); // Generating Access Token after save cuz it shouldn't be saved to DB
			
			// Returning Access & Refresh Token
			return new LoginResponseDTO
			{
				AccessToken = accessToken,
				RefreshToken = refreshtoken.Token
			};
			
		}

		public async Task RevokeRefreshTokenAsync(string refreshToken, int userId)
		{
			var RefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
			if (RefreshToken == null) throw new KeyNotFoundException("Refresh Token Not Found");
			if (RefreshToken.IsRevoked || RefreshToken.ExpiresAt < DateTime.Now) throw new InvalidOperationException("Ivalid Refresh Token, Login Again");
			if (RefreshToken.UserId != userId) throw new UnauthorizedAccessException("you can only logout of your Account");
			await _unitOfWork.RefreshTokens.RevokeTokenAsync(RefreshToken.Token);
			await _unitOfWork.SaveAsync();
		}

		public async Task VerifyEmailAsync(string token)
		{
			var emailVerificationToken = await _unitOfWork.EmailVerificationTokens.GetByVerificationTokenAsync(token);
			if (emailVerificationToken == null) throw new KeyNotFoundException();
			if (emailVerificationToken.IsUsed || emailVerificationToken.ExpiresAt < DateTime.Now) throw new InvalidOperationException();
			var user = await _unitOfWork.Users.GetByIdAsync(emailVerificationToken.UserId);
			if (user == null) throw new KeyNotFoundException();
			user.IsEmailVerified = true;
			emailVerificationToken.IsUsed = true;
			await _unitOfWork.SaveAsync();
		}

		public async Task ForgotPasswordAsync(string email)
		{
			var user = await _unitOfWork.Users.GetByEmailAsync(email);
			if (user == null) return;
			var passwordResetToken = new PasswordResetToken
			{
				Token = _tokenService.GenerateSecureToken(),
				ExpiresAt = DateTime.Now.AddMinutes(15),
				IsUsed = false,
				UserId = user.UserId
			};
			await _unitOfWork.PasswordResetTokens.AddAsync(passwordResetToken);
			await _unitOfWork.SaveAsync();
			await _emailService.SendPasswordResetVerificationAsync(user.Email, passwordResetToken.Token);

		}

		public async Task ResetPasswordAsync(string token, string newPassword)
		{
			var secureToken = await _unitOfWork.PasswordResetTokens.GetByTokenAsync(token);
			if (secureToken == null) throw new KeyNotFoundException();
			if (secureToken.IsUsed || secureToken.ExpiresAt < DateTime.Now) throw new InvalidOperationException();
			var user = await _unitOfWork.Users.GetByIdAsync(secureToken.UserId);
			if (user == null) return;
			if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
				throw new InvalidOperationException("Provide a different Password.");
			var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
			user.PasswordHash = newPasswordHash;
			secureToken.IsUsed = true;
			await _unitOfWork.SaveAsync();
		}

		public async Task ResendVerificationEmailAsync(string email)
		{
			var user = await _unitOfWork.Users.GetByEmailAsync(email);
			if (user == null) return;
			if (user.IsEmailVerified) return;
			var activeTokens = await _unitOfWork.EmailVerificationTokens.GetActiveTokenByUserIdAsync(user.UserId);
			foreach(var token in activeTokens)
			{
				token.IsUsed = true;
			}
			var secureToken = _tokenService.GenerateSecureToken();
			var emailVerificationToken = new EmailVerificationToken()
			{
				Token = secureToken,
				UserId = user.UserId,
				ExpiresAt = DateTime.Now.AddMinutes(60),
				IsUsed = false
			};
			await _unitOfWork.EmailVerificationTokens.AddAsync(emailVerificationToken);
			await _unitOfWork.SaveAsync();
			await _emailService.SendEmailVerificationAsync(user.Email, secureToken);

		}

		public async Task ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if(user == null) throw new KeyNotFoundException();
			if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.CurrentPassword, user.PasswordHash))
				throw new InvalidOperationException("Current Password is Incorrect");
			//if (changePasswordDTO.NewPassword != changePasswordDTO.ConfirmPassword)
			//	throw new InvalidOperationException("New Password and Confirm Password do not match.");
			if (BCrypt.Net.BCrypt.Verify(changePasswordDTO.NewPassword, user.PasswordHash))
				throw new InvalidOperationException("New Password Must be different from Old Password.");
			
			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDTO.NewPassword);

			var activeTokens = await _unitOfWork.RefreshTokens.GetActiveTokenByUserIdAsync(userId);
			if(activeTokens.Any())
			{
				foreach(var token in activeTokens)
			{
				token.IsRevoked = true;
			}
			}
			
			await _unitOfWork.SaveAsync();
		}
	}
}
