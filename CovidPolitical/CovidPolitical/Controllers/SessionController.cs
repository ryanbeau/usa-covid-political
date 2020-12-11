using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using CovidPolitical.Models;
using Microsoft.AspNetCore.Identity;
using CovidPolitical.Entities;
using Microsoft.EntityFrameworkCore;
using CovidPolitical.Handler;
using System.Linq;

namespace CovidPolitical.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : Controller
    {
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly ApplicationDbContext _context;

        public SessionController(JwtTokenGenerator jwtTokenGenerator, ApplicationDbContext context)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost, Route("Login")]
        public async Task<IActionResult> Login(Session session)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "One or more validation errors occurred.",
                    errors = ModelState.Values.SelectMany(m => m.Errors.Select(e => e.ErrorMessage)).ToArray()
                });
            }

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
                token = _jwtTokenGenerator.GenerateJWTTokenFromUser(user),
            });
        }
    }
}
