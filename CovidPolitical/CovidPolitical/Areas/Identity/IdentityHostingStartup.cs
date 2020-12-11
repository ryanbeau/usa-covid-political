using System;
using CovidPolitical.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(CovidPolitical.Areas.Identity.IdentityHostingStartup))]
namespace CovidPolitical.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<CovidPoliticalDbContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("CovidPoliticalDbContextConnection")));

                services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<CovidPoliticalDbContext>();
            });
        }
    }
}