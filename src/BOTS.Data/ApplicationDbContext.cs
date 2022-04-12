namespace BOTS.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    using Models;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Nationality> Nationalities { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}
