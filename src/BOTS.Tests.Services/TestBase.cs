namespace BOTS.Tests.Services
{
    using System;

    using BOTS.Data;
    using BOTS.Data.Infrastructure.Repositories;
    using BOTS.Data.Infrastructure.Repositories.EntityFramework;
    using BOTS.Data.Infrastructure.Transactions;
    using BOTS.Data.Infrastructure.Transactions.EntityFramework;
    using BOTS.Services;
    using BOTS.Services.Currencies.CurrencyRates;
    using BOTS.Services.Infrastructure.Events;
    using BOTS.Services.Mapping;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.DependencyInjection;

    public abstract class TestBase
    {
        protected readonly IServiceProvider serviceProvider;

        public TestBase()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                options.ConfigureWarnings(configuration =>
                {
                    configuration.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                });
            });

            services.AddHttpClient<ThirdPartyCurrencyRateProviderService>();

            services.AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>));
            services.AddScoped<ITransactionManager, EntityFrameworkTransactionManager>();

            services.RegisterServiceLayer();

            services.AddSingleton(typeof(IEventManager<>), typeof(EventManager<>));

            services.AddAutoMapper();

            services.AddMemoryCache();

            this.serviceProvider = services.BuildServiceProvider();
        }
    }
}
