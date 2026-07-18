using JobBoardAPI.Services.Interfaces;
using MimeKit;
using MailKit.Net.Smtp;
using Polly;
using Polly.Retry;
using System.Security.Cryptography;

namespace JobBoardAPI.Services.Implementations
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _config;
		private readonly ILogger<EmailService> _logger;
		private readonly ResiliencePipeline _retryPipeline;
		public EmailService(IConfiguration config, ILogger<EmailService> logger)
		{
			_config = config;
			_logger = logger;
			_retryPipeline = new ResiliencePipelineBuilder()
	.AddRetry(new RetryStrategyOptions
	{
		ShouldHandle = new PredicateBuilder()
			.Handle<SmtpCommandException>()
			.Handle<SmtpProtocolException>()
			.Handle<System.IO.IOException>(),

		MaxRetryAttempts = 3,
		Delay = TimeSpan.FromSeconds(2),
		BackoffType = DelayBackoffType.Exponential,

		OnRetry = args =>
		{
			_logger.LogWarning(
				args.Outcome.Exception,
				"Email sending failed. Retry {RetryAttempt} after {Delay}.",
				args.AttemptNumber + 1,
				args.RetryDelay);

			return ValueTask.CompletedTask;
		}
	})
	.Build();
		}

		private async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			try
			{
				await _retryPipeline.ExecuteAsync(async cancellationtoken =>
				{
					var emailSettings = _config.GetSection("EmailSettings");

					var senderEmail = emailSettings["SenderEmail"] ?? throw new InvalidOperationException("SenderEmail not configured");
					var senderName = emailSettings["SenderName"] ?? "Job Board";
					var appPassword = emailSettings["AppPassword"] ?? throw new InvalidOperationException("AppPassword not configured");

					var message = new MimeMessage();
					message.From.Add(new MailboxAddress(senderName, senderEmail));
					message.To.Add(new MailboxAddress("", toEmail));
					message.Subject = subject;
					message.Body = new TextPart("html") { Text = body };

					using var client = new SmtpClient();
					await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls, cancellationtoken);
					await client.AuthenticateAsync(senderEmail, appPassword, cancellationtoken);
					await client.SendAsync(message, cancellationtoken);
					await client.DisconnectAsync(true, cancellationtoken);
				});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Failed to Send Email.");
				throw;
			}
		}


		public async Task SendEmailVerificationAsync(string toEmail, string verificationToken)
		{
			var subject = "Verify your Email for Job Board";
			var body = $@"
				<h2> EmailVerification </h2>
                <p> Thank you for Registering. Please Verify you email by clicking the link below:</p>
                <a href='https://localhost:7284/api/auth/verify-email?token={verificationToken}'>Verify Email</a>
            <p>This link expires in 1 hours.</p>";

			await SendEmailAsync(toEmail, subject, body);
		}

		public async Task SendPasswordResetVerificationAsync(string toEmail, string verificationToken)
		{
			var subject = "Password Reset - Job Board";
			var body = $@"
            <h2>Password Reset</h2>
            <p>You requested a password reset. Use the token below:</p>
            <p><strong>{verificationToken}</strong></p>
            <p>This token expires in 1 hour.</p>";

			await SendEmailAsync(toEmail, subject, body);
		}
	}
}
