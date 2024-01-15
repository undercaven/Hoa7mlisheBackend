using hoa7mlishe.API.Authorization.DTO;
using hoa7mlishe.API.Database.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace hoa7mlishe.API.Authorization.Helpers
{
    internal static class AuthorizationHelper
    {
        const int TokenLifespan = 100;
        internal const string Issuer = "PolicyEnforcerServer";
        internal const string Audience = "PolicyEnforcerClient";
        const string key = "chuchikmuchik731afigevshiyklyuchhochet256bit";

        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

        /// <summary>
        /// Генерирует accesstoken
        /// </summary>
        /// <returns>токен</returns>
        internal static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }


        /// <summary>
        /// Генерирует accesstoken
        /// </summary>
        /// <returns>токен</returns>
        internal static TokenApiDTO GenerateTokens(User user)
        {
            var claims = new List<Claim> {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role)
            };

            var jwt = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(TokenLifespan),
                signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var result = new TokenApiDTO()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
                RefreshToken = GenerateRefreshToken(),
            };

            return result;
        }

        /// <summary>
        /// Расшифровывает токен
        /// </summary>
        /// <param name="token">токен</param>
        /// <param name="hoursOffset">количество часов, которое токен действителен</param>
        /// <returns> ID пользователя</returns>
        internal static Guid DecypherToken(string token, ILogger logger, int hoursOffset = 3)
        {
            byte[] data = Convert.FromBase64String(token);
            DateTime when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            if (DateTime.Compare(when, DateTime.UtcNow.AddHours(-hoursOffset)) < 0)
            {
                return Guid.Empty;
            }

            var guid = new Guid(data.Skip(8).ToArray());

            return guid;
        }

        /// <summary>
        /// Расшифровывает токен
        /// </summary>
        /// <param name="token">токен</param>
        /// <param name="hoursOffset">количество часов, которое токен действителен</param>
        /// <returns> ID пользователя</returns>
        internal static Guid DecypherToken(string token, ref DateTime when, int hoursOffset = 3)
        {
            byte[] data = Convert.FromBase64String(token);
            when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            if (DateTime.Compare(when, DateTime.Now.AddHours(-hoursOffset)) < 0)
            {
                return Guid.Empty;
            }

            var guid = new Guid(data.Skip(8).ToArray());

            return guid;
        }
    }
}
