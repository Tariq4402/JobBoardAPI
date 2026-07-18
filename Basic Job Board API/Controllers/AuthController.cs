using JobBoardAPI.DTOs;
using JobBoardAPI.Common;
using JobBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace JobBoardAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("Register")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userRegistrationDTO)
		{
			await _authService.RegisterUserAsync(userRegistrationDTO);
			//if (!result) return BadRequest("Registration Failed");
			return Ok("Registration Successful");
		}

		[EnableRateLimiting("AuthPolicy")]
		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
		{
			var result = await _authService.LoginAsync(userLoginDTO);
			//if(result == null) return Unauthorized("Invalid Credentials");
			return Ok(result);
		}

		[HttpPost("refreshtoken")]
		public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequestDTO refreshTokenRequestDTO)
		{
			var result = await _authService.RefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);
			return Ok(result);
		}


		[HttpPatch("revoketoken")]
		[Authorize]
		public async Task<IActionResult> RevokeTokenAsync([FromBody]RefreshTokenRequestDTO refreshTokenRequestDTO)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _authService.RevokeRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken, userId);
			return Ok();
		}

		[HttpGet("verify-email")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> VerifyEmailAsync([FromQuery]string token)
		{
			await _authService.VerifyEmailAsync(token);
			return Ok(new { Message = "Email verified Successfully" });
		}

		[HttpPost("forgot-password")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordDTO forgotPasswordDTO)
		{
			await _authService.ForgotPasswordAsync(forgotPasswordDTO.Email);
			return Ok(new { Message = "If an Account exists with that Email, A password Reset Email has been sent." });
		}

		[HttpPost("reset-password")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ResetPasswordAsync([FromBody]ResetPasswordDTO resetPasswordDTO)
		{
			//if (resetPasswordDTO.Password != resetPasswordDTO.ConfirmPassword)
			//	return BadRequest("Password and ConfirmPassword do not match");

			await _authService.ResetPasswordAsync(resetPasswordDTO.Token, resetPasswordDTO.Password);

			return Ok(new { Message = "Password Reset Successfully." });
		}

		[HttpPost("change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDTO changePasswordDTO)
		{
			int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _authService.ChangePasswordAsync(userId, changePasswordDTO);
			return Ok(new { Message = "Password Changes Successfully." });
		}

		[HttpPost("resend-verification-email")]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailDTO resendVerificationEmailDTO)
		{
			await _authService.ResendVerificationEmailAsync(resendVerificationEmailDTO.Email);

			return Ok(new { Message = "A Verification Email is sent to given Email Address." });
		}
	}
}
