using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AzureTechJwt.Context;
using AzureTechJwt.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AzureTechJwt.Services
{
    public class JwtService : IJwtService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<AuthResponse> GetTokenAsync(AuthRequest authRequest, string ipAddress)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName.Equals(authRequest.UserName)
                                                          && x.Password.Equals(authRequest.Password));
            if (user is null)
                return await Task.FromResult<AuthResponse>(null);
            var tokenString = GenerateToken(user.UserName);
            string refreshToken = GenerateRefreshToken();
            return await SaveTokenDetails(ipAddress, refreshToken, tokenString, user.UserId);
        }

        private async Task<AuthResponse> SaveTokenDetails(string ipAddress, string refreshToken, string tokenString, int userId)
        {
            var userRefreshToken = new UserRefreshToken()
            {
                CreatedDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddMinutes(5),
                IpAddress = ipAddress,
                IsInvalidated = false,
                RefreshToken = refreshToken,
                Token = tokenString,
                UserId = userId
            };
            await _context.UserRefreshTokens.AddAsync(userRefreshToken);
            await _context.SaveChangesAsync();
            return new AuthResponse() {Token = tokenString, RefreshToken = refreshToken,IsSuccess = true};
        }

        public async Task<AuthResponse> GetRefreshTokenAsync(string ipAddress, int userId, string userName)
        {
            var refreshToken = GenerateRefreshToken();
            var accessToken = GenerateToken(userName);
            return await SaveTokenDetails(ipAddress, refreshToken:refreshToken, accessToken, userId);
        }

        public async Task<bool> IsTokenValid(string accessToken, string ipAddress)
        {
            var isValid = _context.UserRefreshTokens.FirstOrDefault(x => x.Token == accessToken &&
                                                                         x.IpAddress == ipAddress) != null;
            return await Task.FromResult(isValid);
        }

        private string GenerateRefreshToken()
        {
            var byteArray = new byte[64];
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                cryptoProvider.GetBytes(byteArray);
                return Convert.ToBase64String(byteArray);
            }
        }

        private string GenerateToken(string userName)
        {
            var jwtKey = _configuration.GetValue<string>("JwtSettings:Key");
            var keyBytes = Encoding.ASCII.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userName)
                }),
                Expires = DateTime.UtcNow.AddSeconds(180),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(descriptor);
            string tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }
    }
}