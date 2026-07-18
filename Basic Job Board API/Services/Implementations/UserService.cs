using AutoMapper;
using JobBoardAPI.Common;
using JobBoardAPI.DTOs;
using JobBoardAPI.Enums;
using JobBoardAPI.Repositories.Interfaces;
using JobBoardAPI.Services.Interfaces;

namespace JobBoardAPI.Services.Implementations
{
	public class UserService : IUserService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UserService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<UserResponseDTO> GetMyProfileAsync(int userId)
		{
			var myProfile = await _unitOfWork.Users.GetByIdAsync(userId);
			if (myProfile == null) throw new KeyNotFoundException("User Not Found");
			//if(myProfile.UserId != userId) throw new UnauthorizedAccessException();
			return _mapper.Map<UserResponseDTO>(myProfile);
		}

		public async Task<IEnumerable<UserResponseDTO>> SearchUserAsync(string userName)
		{
			var userProfile = await _unitOfWork.Users.GetByNameAsync(userName);
			if (!userProfile.Any()) throw new KeyNotFoundException("No User Exists with this UserName");
			return _mapper.Map<IEnumerable<UserResponseDTO>>(userProfile);
		}

		public async Task UpdateMyProfileAsync(int userId, UserUpdateDTO userUpdateDTO)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if (user == null) throw new KeyNotFoundException();

			if (user.Role != Roles.Employer && userUpdateDTO.CompanyName != null) // if user is not Employer he can't set Company Name it will be ignored.
				userUpdateDTO.CompanyName = null;

			if(userUpdateDTO.Name != null) user.Name = userUpdateDTO.Name;
			if(userUpdateDTO.About != null) user.About = userUpdateDTO.About;
			if(userUpdateDTO.CompanyName != null) user.CompanyName = userUpdateDTO.CompanyName;

			_unitOfWork.Users.Update(user);
			await _unitOfWork.SaveAsync();
		}

		public async Task DeleteMyAccountAsync(int userId)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if (user == null) throw new KeyNotFoundException();

			var applications = await _unitOfWork.JobApplications.GetJobApplicationsByUserIdAsync(userId);
			foreach(var app in applications)
			{
				_unitOfWork.JobApplications.Delete(app);
			}

			var jobs = await _unitOfWork.Jobs.GetJobsByIdAsync(userId);
			if (jobs.Any())
			{
				foreach(var job in jobs)
					_unitOfWork.Jobs.Delete(job);
			}
			//foreach (var job in jobs)
			//{
			//	_unitOfWork.Jobs.Delete(job);
			//}

			_unitOfWork.Users.Delete(user);
			await _unitOfWork.SaveAsync();
		}

		public async Task UpdateUserEmailAsync(int userId, string newEmail)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if (user == null) throw new KeyNotFoundException();
			var emailExists = await _unitOfWork.Users.ExistsAsync(u => u.Email == newEmail);
			if (emailExists) throw new InvalidOperationException("This email is already in Use");
			user.Email = newEmail;
			await _unitOfWork.SaveAsync();
		}

		public async Task UpdateUserProfilePictureAsync(int userId, string newProfilePicUrl)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if(user == null ) throw new KeyNotFoundException();
			user.ProfilePictureUrl = newProfilePicUrl;
			await _unitOfWork.SaveAsync();
		}

		public async Task BlockUserAsync(int userId)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if( user == null ) throw new KeyNotFoundException();
			if (user.IsBlocked) throw new InvalidOperationException("This User is Already Blocked");
			user.IsBlocked = true;
			await _unitOfWork.SaveAsync();

		}

		public async Task UnBlockUserAsync(int userId)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if(user == null) throw new KeyNotFoundException();
			if (!user.IsBlocked) throw new InvalidOperationException("This User is Not Blocked");
			user.IsBlocked = false;
			await _unitOfWork.SaveAsync();
		}




	}
}
