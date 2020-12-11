using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CovidPolitical.Handler
{
    public class ValidateJwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public ValidateJwtAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config,
            ApplicationDbContext context)
            : base(options, logger, encoder, clock)
        {
            _config = config;
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // skip authentication if endpoint has [AllowAnonymous] attribute
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            // get authorization
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"])),
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidAudience = _config["Jwt:Audience"],
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    }, out SecurityToken securityToken);

                    // get Claims
                    var jwtSecurityToken = (JwtSecurityToken)securityToken;
                    var username = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                    var role = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
                    var accessToken = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    // check if User has matching access token
                    if (await _context.Users.AnyAsync(u => u.Username == username && u.AccessToken != Guid.Empty && u.AccessToken.ToString() == accessToken))
                    {
                        var claims = new[] {
                            new Claim(ClaimTypes.Name, username),
                            new Claim(ClaimTypes.Role, role),
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, nameof(ValidateJwtAuthenticationHandler));

                        var ticket = new AuthenticationTicket(
                            new ClaimsPrincipal(claimsIdentity), Scheme.Name);

                        return AuthenticateResult.Success(ticket);
                    }
                }
                catch
                {

                }
            }

            return AuthenticateResult.Fail("Token failed validation");
        }
    }
}
