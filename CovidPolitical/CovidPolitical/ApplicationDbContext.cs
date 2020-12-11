using CovidPolitical.Entities;
using Microsoft.EntityFrameworkCore;

namespace CovidPolitical
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
