using JobBoardAPI.Data;
using Microsoft.Extensions.Hosting;

namespace JobBoardAPI.BackgroundServices
{
	public class RefreshTokenCleanupService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<RefreshTokenCleanupService> _logger;

		public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stopppingToken)
		{
			while(!stopppingToken.IsCancellationRequested)
			{
				using(var scope = _scopeFactory.CreateScope())
				{
					var context = scope.ServiceProvider.GetRequiredService<JobBoardDBContext>();
					var expiredTokens = context.RefreshTokens.Where(r => r.IsRevoked || r.ExpiresAt < DateTime.UtcNow);
					context.RefreshTokens.RemoveRange(expiredTokens);
					var expiredVerificationTokens = context.EmailVerificationTokens
	                    .Where(e => e.ExpiresAt < DateTime.UtcNow || e.IsUsed);
					context.EmailVerificationTokens.RemoveRange(expiredVerificationTokens);

					var expiredResetTokens = context.PasswordResetTokens
						.Where(p => p.ExpiresAt < DateTime.UtcNow || p.IsUsed);
					context.PasswordResetTokens.RemoveRange(expiredResetTokens);
					await context.SaveChangesAsync();
					_logger.LogInformation("Expired Refresh Tokens Cleaned Up.");
				}
				await Task.Delay(TimeSpan.FromHours(24), stopppingToken);
			}
		}
	}
}
