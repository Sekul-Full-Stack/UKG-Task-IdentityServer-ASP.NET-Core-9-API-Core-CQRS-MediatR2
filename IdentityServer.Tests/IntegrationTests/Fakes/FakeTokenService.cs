namespace IdentityServer.Tests.IntegrationTests.Fakes
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    using Microsoft.IdentityModel.Tokens;

    using IdentityServer.Application.Interfaces;
    using IdentityServer.Application.Results;
    using IdentityServer.Domain.Models;

    public class FakeTokenService : ITokenService
    {
        public bool ForceFailure { get; set; } = false;
        public string? FailureMessage { get; set; } = "Token generation failed";

        private readonly string _secretKey = "super-secret-key-1234567890-abcdef";

        public IdentityResult<string> GenerateToken(string userId, User user, IEnumerable<string> roles)
        {
            if (ForceFailure)
            {
                return IdentityResult<string>.Failure(FailureMessage);
            }
             
            var claims = new List<Claim>
        {
            new Claim("userId", userId),
            new Claim("userName", user.UserName ?? ""),
            new Claim("email", user.Email ?? "")
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim("roles", role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "ukg",
                audience: "my-app-users",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return IdentityResult<string>.Success(tokenString);
        }
    }  
}
