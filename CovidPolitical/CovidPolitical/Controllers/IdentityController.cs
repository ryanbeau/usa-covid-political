using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CovidPolitical.Controllers
{
    public class IdentityController : Controller
    {
        public IActionResult Index()
        {
            //return RedirectToRoute("~/Views/Shared/_LoginPartial");
            return RedirectToPage("Login"); // ("~/Areas/Identity/Pages/Account/Login");
        }
    }
}
