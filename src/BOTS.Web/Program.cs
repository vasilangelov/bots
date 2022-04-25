using Microsoft.EntityFrameworkCore;

using BOTS.Data;
using BOTS.Data.Models;
using BOTS.Web.BackgroundServices;
using BOTS.Web.Extensions;
using BOTS.Services;
using BOTS.Data.Seeding;
using BOTS.Web.Hubs;
using BOTS.Services.Data.Common;
using BOTS.Services.Data.CurrencyPairs;
using BOTS.Services.Data.TradingWindows;

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

builder.Services.Configure<ApplicationUserOptions>(builder.Configuration.GetSection("ApplicationUserOptions"));

builder.Services.AddHttpClient("CurrencyAPI", options =>
 {
     options.BaseAddress = new Uri("https://api.exchangerate.host/latest");
 });

builder.Services.AddHostedService<CurrencyHostedService>();

builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddTransient<ICurrencyProviderService, CurrencyProviderService>();
builder.Services.AddTransient<ICurrencyPairService, CurrencyPairService>();
builder.Services.AddTransient<ITradingWindowOptionService, TradingWindowOptionService>();
builder.Services.AddTransient<ITradingWindowService, TradingWindowService>();

// Configure pipeline...
var app = builder.Build();
await app.MigrateDatabaseAsync();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
