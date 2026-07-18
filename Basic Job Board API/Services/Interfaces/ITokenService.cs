using JobBoardAPI.Entities;

namespace JobBoardAPI.Services.Interfaces
{
	public interface ITokenService
	{
		string GenerateToken(User user);
		RefreshToken GenerateRefreshToken(int userId);
		string GenerateSecureToken();
	}
}
