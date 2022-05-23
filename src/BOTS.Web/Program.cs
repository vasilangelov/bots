using System.Text.Json.Serialization;

using BOTS.Data;
using BOTS.Data.Infrastructure.Repositories.EntityFramework;
using BOTS.Data.Infrastructure.Transactions;
using BOTS.Data.Infrastructure.Transactions.EntityFramework;
using BOTS.Data.Models;
using BOTS.Data.Repositories;
using BOTS.Services;
using BOTS.Services.Currencies.CurrencyRates;
using BOTS.Services.Mapping;
using BOTS.Web.BackgroundServices;
using BOTS.Web.Extensions;
using BOTS.Web.Hubs;
using BOTS.Web.Models.ViewModels;

using Microsoft.EntityFrameworkCore;

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
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>));
builder.Services.AddScoped<ITransactionManager, EntityFrameworkTransactionManager>();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        options.User.RequireUniqueEmail = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;

    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services
    .AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters
           .Add(new JsonStringEnumConverter());
    });

builder.Services.AddHttpClient<ThirdPartyCurrencyRateProviderService>();

builder.Services.AddHostedService<CurrencyRateGeneratorBackgroundService>();
builder.Services.AddHostedService<CurrencyRateBackgroundService>();
builder.Services.AddHostedService<BettingOptionBackgroundService>();
builder.Services.AddHostedService<CurrencyRateStatBackgroundService>();

builder.Services.RegisterServiceLayer();

// TODO: register service layer logic...

builder.Services.AddAutoMapper(typeof(ErrorViewModel).Assembly);

var app = builder.Build();

// Setup application initials...
await app.MigrateDatabaseAsync();
await app.SeedDatabaseAsync();

await app.SeedCurrenciesAsync();

// Configure pipeline...
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
    app.MapHub<TradingHub>("/Currencies/Live");
});

app.Run();
