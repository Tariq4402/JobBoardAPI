using JobBoardAPI.Entities;

namespace JobBoardAPI.Entities
{
	public class PasswordResetToken
	{
		public int Id { get; set; }
		public string Token { get; set; } = string.Empty;
		public DateTime ExpiresAt { get; set; }
		public bool IsUsed { get; set; }
		public int UserId { get; set; }
		public User? User { get; set; }
	}
}
