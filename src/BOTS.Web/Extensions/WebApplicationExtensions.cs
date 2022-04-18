using BOTS.Data;
using Microsoft.EntityFrameworkCore;

namespace BOTS.Web.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task MigrateDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.MigrateAsync();
        }
    }
}
