using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using CovidPolitical.Models;
using Microsoft.AspNetCore.Identity;
using CovidPolitical.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace CovidPolitical.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public SessionController(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost, Route("Login")]
        public async Task<IActionResult> Login(Session session)
        {
            var hasher = new PasswordHasher<User>();
            User user = await _context.Users.SingleOrDefaultAsync(u => u.Username == session.Username);
            if (user == null)
            {
                // register user - TODO: only for this assignment to make it easier for everyone testing it
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = session.Username,
                };
                user.PasswordHash = hasher.HashPassword(user, session.Password);

                // add user
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else if (hasher.VerifyHashedPassword(user, user.PasswordHash, session.Password) != PasswordVerificationResult.Success)
            {
                return new UnauthorizedObjectResult(new { Message = "You are not authorized." });
            }

            if (user.AccessToken == Guid.Empty) // user is not already logged in
            {
                // generate access token
                user.AccessToken = Guid.NewGuid();

                // update user
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                token = GenerateJWTToken(session, user.AccessToken),
            });
        }

        private string GenerateJWTToken(Session session, Guid jti)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, session.Username),
                new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
                new Claim(ClaimTypes.Role, "Member"),
                new Claim(ClaimTypes.Name, session.Username),
            };
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
