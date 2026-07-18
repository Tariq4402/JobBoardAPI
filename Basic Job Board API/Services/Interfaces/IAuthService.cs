using JobBoardAPI.DTOs;
using JobBoardAPI.Entities;
namespace JobBoardAPI.Services.Interfaces
{
	public interface IAuthService
	{
		// interface members are public by default.
		Task RegisterUserAsync(UserRegistrationDTO userRegistrationDTO);
		
		Task<LoginResponseDTO> LoginAsync(UserLoginDTO userLoginDTO);

		Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken);
		Task RevokeRefreshTokenAsync(string refreshToken, int userId);

		Task VerifyEmailAsync(string token);
		Task ForgotPasswordAsync(string email);
		Task ResetPasswordAsync(string token, string newPassword);
		Task ResendVerificationEmailAsync(string email);
		Task ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO);
	}
}
