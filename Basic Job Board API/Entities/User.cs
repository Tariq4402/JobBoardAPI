using JobBoardAPI.Entities;

namespace JobBoardAPI.Entities
{
	public class User
	{
		public int UserId { get; set; }
		public required string Name { get; set; }
		public string? About { get; set; }
		public required string Email { get; set; }
		public required string PasswordHash { get; set; }
		public required string Role { get; set; }
		public string? CompanyName { get; set; }
		public string? ProfilePictureUrl { get; set; }
		public bool IsBlocked { get; set; } = false;
		public bool IsEmailVerified { get; set; } = false;
		public ICollection<Job>? Jobs { get; set; } // Nav
		public ICollection<JobApplication>? JobApplications { get; set; } // Nav
		public ICollection<RefreshToken>? RefreshTokens { get; set; }
		public ICollection<EmailVerificationToken>? EmailVerificationTokens { get; set; }
		public ICollection<PasswordResetToken>? PasswordResetTokens { get; set; }
	}
}
