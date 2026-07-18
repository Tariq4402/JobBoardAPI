using JobBoardAPI.Data;
using JobBoardAPI.Exceptions;
using JobBoardAPI.Mappings;
using JobBoardAPI.Repositories.Implementations;
using JobBoardAPI.Repositories.Interfaces;
using JobBoardAPI.Services.Implementations;
using JobBoardAPI.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text;
using JobBoardAPI.BackgroundServices;


try
{
	var builder = WebApplication.CreateBuilder(args);

	// Add services to the container.

	builder.Services.AddControllers(options =>
	{
		options.SuppressAsyncSuffixInActionNames = false;
	})
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
	});

	var jwtSettings = builder.Configuration.GetSection("JwtSettings");
	var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

	builder.Services.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	}).AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtSettings["Issuer"],
			ValidAudience = jwtSettings["Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(key),
			ClockSkew = TimeSpan.Zero // no grace period on token expiration
		};
	});
	builder.Services.AddMemoryCache();
	builder.Services.AddAuthorization();
	builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
	builder.Services.AddValidatorsFromAssemblyContaining<Program>();
	builder.Services.AddDbContext<JobBoardDBContext>
		(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
	builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
	builder.Services.AddScoped<IJobRepository, JobRepository>();
	builder.Services.AddScoped<IUserRepository, UserRepository>();
	builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
	builder.Services.AddScoped<IEmailService, EmailService>();
	builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
	builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
	builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
	builder.Services.AddScoped<ITokenService, TokenService>();
	builder.Services.AddScoped<IAuthService, AuthService>();
	builder.Services.AddScoped<IJobService, JobService>();
	builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
	builder.Services.AddScoped<IUserService, UserService>();
	builder.Services.AddHostedService<RefreshTokenCleanupService>();
	builder.Services.AddControllers(options =>
	{
		options.Filters.Add<GlobalExceptionFilter>();
		options.SuppressAsyncSuffixInActionNames = false;
	});

	builder.Services.AddCors(options =>
	{
		options.AddPolicy("AllowFrontEnd", policy =>
		{
			policy.WithOrigins("http://localhost:3000", "http://localhost:4200") // For FrontEnd
			.AllowAnyHeader()
			.AllowAnyMethod();
		});
	});

	builder.Services.AddEndpointsApiExplorer();

	builder.Services.AddSwaggerGen(options =>
	{
		options.SwaggerDoc("v1", new OpenApiInfo
		{
			Title = "Job Board API",
			Version = "v1",
			Description = "A Job Board REST API built with ASP.NET Core"
		});

		options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
		{
			Name = "Authorization",
			Type = SecuritySchemeType.Http,
			Scheme = "Bearer",
			BearerFormat = "JWT",
			In = ParameterLocation.Header,
			Description = "Enter your JWT token here"
		});

		options.AddSecurityRequirement(document => new()
		{
			[new OpenApiSecuritySchemeReference("Bearer", document)] = []
		});
	});

	Log.Logger = new LoggerConfiguration()
		.MinimumLevel.Information()
		.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
		.MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
		.WriteTo.Console()
		.WriteTo.File("Logs/jobboard-.log", rollingInterval: RollingInterval.Day)
		.CreateLogger();

	builder.Host.UseSerilog();

	builder.Services.AddRateLimiter(options =>
	{
		options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
		{
			limiterOptions.PermitLimit = 5;
			limiterOptions.Window = TimeSpan.FromMinutes(1);
			limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			limiterOptions.QueueLimit = 0;
		});

		options.AddFixedWindowLimiter("GeneralPolicy", limiterOptions =>
		{
			limiterOptions.PermitLimit = 15;
			limiterOptions.Window = TimeSpan.FromMinutes(1);
			limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			limiterOptions.QueueLimit = 0;
		});

		options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	});

	var app = builder.Build();

	// Configure the HTTP request pipeline.

	app.UseSwagger();
	app.UseSwaggerUI();

	app.UseHttpsRedirection();

	app.UseCors("AllowFrontEnd");

	app.UseRateLimiter();

	app.UseAuthentication();

	app.UseAuthorization();

	app.MapControllers();

	app.Run();
}
catch(Exception ex)
{
	Log.Fatal(ex, "Application Failed to Start.");
}
finally
{
	Log.CloseAndFlush();
}
