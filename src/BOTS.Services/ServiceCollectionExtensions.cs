namespace BOTS.Services
{
    using BOTS.Services.Common;

    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServiceLayer(this IServiceCollection serviceCollection)
            => serviceCollection
                .Scan(scan => scan.FromAssemblyOf<TransientServiceAttribute>()
                                  .AddClasses(classes => classes
                                        .WithAttribute(typeof(TransientServiceAttribute)))
                                  .AsImplementedInterfaces()
                                  .WithTransientLifetime());
    }
}
