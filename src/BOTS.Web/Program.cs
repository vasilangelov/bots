using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

using BOTS.Data;
using BOTS.Data.Models;
using BOTS.Web.BackgroundServices;
using BOTS.Web.Extensions;
using BOTS.Services.Models;
using BOTS.Web.Hubs;
using BOTS.Services.Data.Common;
using BOTS.Services.Data.CurrencyPairs;
using BOTS.Services.Data.TradingWindows;
using BOTS.Services.Mapping;
using BOTS.Services.Data.Nationalities;
using BOTS.Services.Currencies;
using BOTS.Services.Data.Bets;
using BOTS.Services.Data.Users;
using BOTS.Web.Models.ViewModels;
using BOTS.Services.Data.UserPresets;
using BOTS.Services.Data.ApplicationSettings;

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

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

}).AddEntityFrameworkStores<ApplicationDbContext>();

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
builder.Services.AddHostedService<TradingWindowBackgroundService>();
builder.Services.AddHostedService<CurrencyRateStatBackgroundService>();

builder.Services.AddAutoMapper(typeof(ErrorViewModel).Assembly, typeof(TradingWindowOptionInfo).Assembly);

builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddTransient<IApplicationSettingService, ApplicationSettingService>();
builder.Services.AddTransient<INationalityService, NationalityService>();
builder.Services.AddTransient<ICurrencyPairService, CurrencyPairService>();
builder.Services.AddTransient<ITradingWindowService, TradingWindowService>();
builder.Services.AddTransient<ITradingWindowOptionService, TradingWindowOptionService>();
builder.Services.AddTransient<IBetService, BetService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IUserPresetService, UserPresetService>();
builder.Services.AddTransient<ThirdPartyCurrencyRateProviderService>();

builder.Services.AddSingleton<ICurrencyRateProviderService, CurrencyGeneratorService>();
builder.Services.AddSingleton<ICurrencyRateGeneratorService>(x => (ICurrencyRateGeneratorService)x.GetRequiredService<ICurrencyRateProviderService>());
builder.Services.AddSingleton<ICurrencyRateStatProviderService, CurrencyRateStatGeneratorService>();

// Configure pipeline...
var app = builder.Build();

await app.MigrateDatabaseAsync();
await app.SeedDatabaseAsync();

await app.SeedCurrenciesAsync();

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
