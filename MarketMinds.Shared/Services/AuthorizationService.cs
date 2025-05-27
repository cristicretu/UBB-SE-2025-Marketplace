using Microsoft.IdentityModel.Tokens;
using MarketMinds.Shared.Helper;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MarketMinds.Shared.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        public AuthorizationService() { }

        public string GenerateJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            string jwtKey = AppConfig.GetJwtTokenKey();
            string jwtIssuer = AppConfig.GetJwtTokenIssuer();

            Debug.WriteLine("JWT Key found: " + jwtKey);

            if (jwtKey == "NOT FOUND")
            {
                throw new InvalidOperationException("JWT Key not found in configuration");
            }

            var key = Encoding.UTF8.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = jwtIssuer,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
