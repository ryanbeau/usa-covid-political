using CovidPolitical.Handler;
using CovidPolitical.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace CovidPolitical.Controllers
{
    public class MapController : Controller
    {
        private readonly IConfiguration _config;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly ApplicationDbContext _context;

        private readonly string _guestUserName;
        private readonly string _guestAccessToken;

        public MapController(IConfiguration config, JwtTokenGenerator jwtTokenGenerator, ApplicationDbContext context)
        {
            _config = config;
            _jwtTokenGenerator = jwtTokenGenerator;
            _context = context;

            _guestUserName = _config["Guest:UserName"];
            _guestAccessToken = _config["Guest:AccessToken"];
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            // return a Guest cookie - user cannot access API without it
            string jwtToken = $"Bearer {_jwtTokenGenerator.GenerateJWTToken(_guestUserName, _guestAccessToken, "Guest")}";
            Response.Cookies.Append("Authorization", jwtToken, new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(1),
            });

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
