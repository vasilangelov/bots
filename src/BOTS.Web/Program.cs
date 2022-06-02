using System.Text.Json.Serialization;

using BOTS.Data;
using BOTS.Data.Infrastructure.Repositories;
using BOTS.Data.Infrastructure.Repositories.EntityFramework;
using BOTS.Data.Infrastructure.Transactions;
using BOTS.Data.Infrastructure.Transactions.EntityFramework;
using BOTS.Data.Models;
using BOTS.Services;
using BOTS.Services.Balance.User.Events;
using BOTS.Services.Currencies.CurrencyRates;
using BOTS.Services.Infrastructure.Events;
using BOTS.Services.Mapping;
using BOTS.Services.Trades.TradingWindows.Events;
using BOTS.Web.BackgroundServices;
using BOTS.Web.Extensions;
using BOTS.Web.Hubs.Trading;
using BOTS.Web.Hubs.Trading.Events;
using BOTS.Web.Models.ViewModels;
using BOTS.Web.Resources;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure services...
var mvcBuilder = builder.Services.AddControllersWithViews();
mvcBuilder.Services.AddRazorPages();
mvcBuilder
    .AddViewLocalization(opt =>
    {
        opt.ResourcesPath = "Resources";
    })
    .AddDataAnnotationsLocalization(opt =>
    {
        opt.DataAnnotationLocalizerProvider = (t, f) =>
        {
            var fac = f.Create(typeof(ValidationMessages));

            return fac;
        };
    });

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
    .AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

builder.Services
    .AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters
           .Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddLocalization();

builder.Services
    .Configure<RequestLocalizationOptions>(options =>
    {
        options.SetDefaultCulture("en-US");

        options.AddSupportedCultures("en-US", "bg-BG");
        options.AddSupportedUICultures("en-US", "bg-BG");
    });

builder.Services.AddHttpClient<ThirdPartyCurrencyRateProviderService>();

builder.Services.AddHostedService<CurrencyRateGeneratorBackgroundService>();
builder.Services.AddHostedService<CurrencyRateBackgroundService>();
builder.Services.AddHostedService<BettingOptionBackgroundService>();
builder.Services.AddHostedService<CurrencyRateStatBackgroundService>();

builder.Services.RegisterServiceLayer();

builder.Services.AddSingleton(typeof(IEventManager<>), typeof(EventManager<>));

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
app.UseRequestLocalization("en-US", "bg-BG");
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(app =>
{
    app.MapDefaultControllerRoute();
    app.MapRazorPages();
    app.MapHub<TradingHub>("/Currencies/Live", (options) =>
    {
        var balanceEventManager = app.ServiceProvider.GetRequiredService<IEventManager<UpdateBalanceEvent>>();

        balanceEventManager.Subscribe<UpdateBalanceEventHandler>();

        var tradingWindowEventManager = app.ServiceProvider.GetRequiredService<IEventManager<TradingWindowClosedEvent>>();

        tradingWindowEventManager.Subscribe<TradingWindowClosedEventHandler>();
    });
});

app.Run();
