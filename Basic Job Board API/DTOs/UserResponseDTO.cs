using JobBoardAPI.Entities;

namespace JobBoardAPI.DTOs
{
	public class UserResponseDTO
	{
		public int UserId { get; set; }
		public required string Name { get; set; }
		public required string Role { get; set; }

		public string? About { get; set; }
		public required string Email { get; set; }
		public string? CompanyName { get; set; }
		public string? ProfilePictureUrl { get; set; }
	}
}
