using Microsoft.EntityFrameworkCore;
using JobBoardAPI.Entities;

namespace JobBoardAPI.Data
{
	public class JobBoardDBContext : DbContext
	{
		public JobBoardDBContext(DbContextOptions<JobBoardDBContext> options) : base(options)
		{

		}

		public DbSet<User> Users { get; set; }
		public DbSet<Job> Jobs { get; set; }
		public DbSet<JobApplication> JobApplications { get; set; }

		public DbSet<RefreshToken> RefreshTokens { get; set; }
		
		public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

		public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Job>()
				.Property(j => j.MaxSalary)
				.HasPrecision(18, 2);

			modelBuilder.Entity<User>()
				.HasMany(u => u.RefreshTokens)
				.WithOne(r => r.User)
				.HasForeignKey(u => u.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<User>()
				.HasMany(u => u.EmailVerificationTokens)
				.WithOne(r => r.User)
				.HasForeignKey(u => u.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<User>()
				.HasMany(u => u.PasswordResetTokens)
				.WithOne(r => r.User)
				.HasForeignKey(u => u.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Job>()
				.Property(j => j.MinSalary)
				.HasPrecision(18, 2);

			modelBuilder.Entity<User>()
				.HasMany(u => u.Jobs)
				.WithOne(j => j.Employer)
				.HasForeignKey(u => u.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<User>()
				.HasMany(j => j.JobApplications)
				.WithOne(k => k.Applicant)
				.HasForeignKey(j => j.UserId)
				.OnDelete(DeleteBehavior.Restrict);

		}
	}
}
