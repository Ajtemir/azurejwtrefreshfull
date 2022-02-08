using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AzureTechJwt.Context;
using AzureTechJwt.Entities;
using AzureTechJwt.Services;
using Microsoft.AspNetCore.Mvc;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AzureTechJwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _context;

        public AccountController(IJwtService jwtService, ApplicationDbContext context)
        {
            _jwtService = jwtService;
            _context = context;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AuthToken([FromBody] AuthRequest authRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponse(){IsSuccess = false,Reason = "UserName and Password must be provided."});
            var authResponse = await _jwtService.GetTokenAsync(authRequest,HttpContext.Connection.RemoteIpAddress?.ToString());
            if (authResponse is null)
                return Unauthorized();
            return Ok(authResponse);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponse{IsSuccess = false, Reason = "Tokens must be provided"});
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var token = GetJwtToken(request.ExpiredToken);
            var userRefreshToken = _context.UserRefreshTokens.FirstOrDefault(
                x => x.IsInvalidated == false && x.Token == request.ExpiredToken
                                              && x.RefreshToken == request.RefreshToken
                                              && x.IpAddress == ipAddress);
            AuthResponse response = ValidateDetails(token, userRefreshToken);
            if (!response.IsSuccess)
                return BadRequest();
            userRefreshToken.IsInvalidated = true;
            _context.UserRefreshTokens.Update(userRefreshToken);
            await _context.SaveChangesAsync();

            var userName = token.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.NameId)?.Value;
            var authResponse = _jwtService.GetRefreshTokenAsync(ipAddress,userRefreshToken.UserId,userName);
            return Ok(authResponse);
        }

        private AuthResponse ValidateDetails(JwtSecurityToken token, UserRefreshToken userRefreshToken)
        {
            if (userRefreshToken == null)
                return new AuthResponse() {IsSuccess = false, Reason = "Invalid Token Details."};
            if (token.ValidTo > DateTime.Now)
                return new AuthResponse() {IsSuccess = false, Reason = "Token not expired."};
            if (!userRefreshToken.IsActive)
                return new AuthResponse() {IsSuccess = false, Reason = "Refresh Token Expired."};
            return new AuthResponse() {IsSuccess = true};


        }

        private JwtSecurityToken GetJwtToken(string expiredToken)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            return tokenHandler.ReadJwtToken(expiredToken);
        }
    }
}