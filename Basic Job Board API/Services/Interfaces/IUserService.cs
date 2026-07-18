using JobBoardAPI.DTOs;
using JobBoardAPI.Enums;

namespace JobBoardAPI.Services.Interfaces
{
	public interface IUserService
	{
		Task<UserResponseDTO> GetMyProfileAsync(int userId);
		Task<IEnumerable<UserResponseDTO>> SearchUserAsync(string userName);
		Task UpdateMyProfileAsync(int userId, UserUpdateDTO userUpdateDTO);
		Task DeleteMyAccountAsync(int userId);
		Task UpdateUserEmailAsync(int userId, string newEmail);
		Task UpdateUserProfilePictureAsync(int userId, string newProfilePicUrl);

		Task BlockUserAsync(int userId);
		Task UnBlockUserAsync(int userId);
	}
}
