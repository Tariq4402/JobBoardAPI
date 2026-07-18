using JobBoardAPI.DTOs;
using JobBoardAPI.Enums;
using JobBoardAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JobBoardAPI.Common;
using Microsoft.AspNetCore.RateLimiting;

namespace JobBoardAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpGet("MyProfile")]
		[Authorize]
		public async Task<IActionResult> GetMyProfileAsync()
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			var myProfile = await _userService.GetMyProfileAsync(userId);
			return Ok(myProfile);
		}

		[HttpGet("Search")]
		[Authorize]
		public async Task<IActionResult> SearchUserAsync([FromQuery]string userName)
		{
			var userProfile = await _userService.SearchUserAsync(userName);
			return Ok(userProfile);
		}

		[HttpPut("UpdateMyProfile")]
		[Authorize]
		public async Task<IActionResult> UpdateProfileAsync([FromBody]UserUpdateDTO userUpdateDTO)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _userService.UpdateMyProfileAsync(userId, userUpdateDTO);
			//if (result == Enums.ServiceResult.UnAuthorized) return Forbid();
			return Ok("ProfileUpdated SuccessFully");
		}

		[HttpDelete("DeleteMyAccount")]
		[Authorize]
		public async Task<IActionResult> DeletMyAccountAsync()
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _userService.DeleteMyAccountAsync(userId);
			//if (result == Enums.ServiceResult.UnAuthorized) return Forbid();
			return Ok("Your Account is Deleted");
		}

		[HttpPut("UpdateEmail")]
		[Authorize]
		public async Task<IActionResult> UpdateUserEmailAsync([FromBody]EmailUpdateDTO newEmail)
		{
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			var updatedEmail = newEmail.Email;
			await _userService.UpdateUserEmailAsync(userId, updatedEmail);
			return Ok("Email Updated Successfully");
		}

		[HttpPatch("UpdateProfilePic")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> UpdateUserProfilePicAsync(IFormFile file)
		{
			// Null or empty file check
			if (file == null || file.Length == 0) return BadRequest("No File Provided");

			// File SIze Check
			if (file.Length > 2 * 1024 * 1024) return BadRequest("Picture Size must be lower than 2MB");

			// Extension Check
			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
			var extension = Path.GetExtension(file.FileName).ToLower();
			if (!allowedExtensions.Contains(extension)) return BadRequest("Only .jpg, .jpeg, .png, .webp files are Allowed");

			// Mime Type Check
			var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
			if (!allowedMimeTypes.Contains(file.ContentType.ToLower())) return BadRequest("Invalid File Type");

			// Save To Disk
			var fileName = Guid.NewGuid().ToString() + extension;
			var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile-pictures");
			Directory.CreateDirectory(folderPath);
			var filePath = Path.Combine(folderPath, fileName);

			using(var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			// Generate Url
			var newProfilePicUrl = $"{Request.Scheme}://{Request.Host}/uploads/profile-pictures/{fileName}";

			// validating User
			var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
			await _userService.UpdateUserProfilePictureAsync(userId, newProfilePicUrl);
			return Ok(new {ProfilePictureUrl = newProfilePicUrl}); // Replacing ProfilePictureUrl
		}

		[HttpPatch("BlockUser/{userId}")]
		[Authorize(Roles = Roles.Admin)]
		public async Task<IActionResult> BlockUserAsync(int userId)
		{
			await _userService.BlockUserAsync(userId);
			return Ok("User Blocked Successfully");

		}

		[HttpPatch("UnblockUser/{userId}")]
		[Authorize(Roles = Roles.Admin)]
		public async Task<IActionResult> UnBlockUserUserAsync(int userId)
		{
			await _userService.UnBlockUserAsync(userId);
			return Ok("User UnBlocked SuccessFully");
		}
	}
}
