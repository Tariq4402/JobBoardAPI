namespace JobBoardAPI.Services.Interfaces
{
	public interface IEmailService
	{
		Task SendEmailVerificationAsync(string toEmail, string verificationToken);
		Task SendPasswordResetVerificationAsync(string toEmail, string resetToken);
	}
}
