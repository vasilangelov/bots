using System.Text.Json;
using Microsoft.EntityFrameworkCore;

using BOTS.Data;
using BOTS.Data.Models;
using BOTS.Web.BackgroundServices;
using BOTS.Services;
using BOTS.Data.Seeding;
using BOTS.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configure services...
var mvcBuilder = builder.Services.AddControllersWithViews();
mvcBuilder.Services.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSignalR();

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.Configure<ApplicationUserOptions>(builder.Configuration.GetSection("ApplicationUserOptions"));

// TODO: extract name constants...
builder.Services.AddHttpClient("CurrencyApi", options =>
 {
     options.BaseAddress = new Uri("https://api.exchangerate.host/latest");
 });
builder.Services.AddSingleton<ICurrencyProviderService, CurrencyProviderService>();
builder.Services.AddHostedService<CurrencyHostedService>();
builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

// Configure pipeline...
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();

    await ApplicationDbContextSeeder.SeedAsync(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(app =>
{
    app.MapDefaultControllerRoute();
    app.MapRazorPages();
    app.MapHub<CurrencyHub>("/Currencies/Live");
});

app.Run();
