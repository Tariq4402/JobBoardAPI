namespace JobBoardAPI.DTOs
{
	public class ResetPasswordDTO
	{
		public string Token { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string ConfirmPassword {  get; set; } = string.Empty;
	}
}
