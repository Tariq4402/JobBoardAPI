using JobBoardAPI.Entities;
using JobBoardAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace JobBoardAPI.Services.Implementations
{
	public class TokenService : ITokenService
	{
		private readonly IConfiguration _config;
		public TokenService(IConfiguration config)
		{
			_config = config;
		}

		public string GenerateToken(User user)
		{
			var jwtSettings = _config.GetSection("JwtSettings"); // get the JWT settings from appsettings.json
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)); // convert key to bytes
			var Credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // create signing credentials using key and Securityalgorithm

			var claims = new List<Claim> // create a list of claims to include in the token
			{
				new Claim(JwtRegisteredClaimNames.Email, user.Email), // add the user's email as a claim
				new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // add the user's ID as a claim
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // add a unique identifier for the token
				new Claim(ClaimTypes.Role, user.Role) // add the user's role as a claim
			};

			// create the token using the claims, signing credentials, and expiration time
			var Token = new JwtSecurityToken(
				issuer: jwtSettings["Issuer"],
				audience: jwtSettings["Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"]!)),
				signingCredentials: Credentials);

			return new JwtSecurityTokenHandler().WriteToken(Token); // return the token as a string
		}

			public RefreshToken GenerateRefreshToken(int userId)
			{
				return new RefreshToken
				{
					Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
					ExpiresAt = DateTime.Now.AddDays(7),
					IsRevoked = false,
					UserId = userId
				};
		}

		public string GenerateSecureToken()
		{
			return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
		}

	}
}
