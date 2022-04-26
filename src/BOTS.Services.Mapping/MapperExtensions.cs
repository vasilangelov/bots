namespace BOTS.Services.Mapping
{
    using AutoMapper;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    public static class MapperExtensions
    {
        private const string ModelCreationExceptionMessage = "Cannot create instance of type {0}. It should have a parameterless constructor";

        public static void AddAutoMapper(this IServiceCollection serviceCollection, params Assembly[] assembliesToScan)
        {
            serviceCollection.AddSingleton(sp => AutoMapperFactory(assembliesToScan));
        }

        private record class MappingInfo(Type From, Type To);

        private static IMapper AutoMapperFactory(params Assembly[] assembliesToScan)
        {
            var configurationExpression = new MapperConfigurationExpression();

            foreach (var assembly in assembliesToScan)
            {
                var mappingInfos = assembly
                    .GetTypes()
                    .SelectMany(t => t.GetInterfaces()
                                      .Where(i => i.IsGenericType &&
                                                  i.GetGenericTypeDefinition() == typeof(IMapFrom<>))
                                      .Select(i => new MappingInfo(i.GenericTypeArguments[0], t))
                                      .ToArray())
                    .ToArray();

                foreach (var mappingInfo in mappingInfos)
                {
                    configurationExpression.CreateMap(mappingInfo.From, mappingInfo.To);
                }

                var customMappings = assembly
                    .GetTypes()
                    .Where(x => x.IsAssignableTo(typeof(ICustomMap)))
                    .ToArray();

                foreach (var customMap in customMappings)
                {
                    ICustomMap? instance = (ICustomMap?)Activator.CreateInstance(customMap);

                    if (instance is null)
                    {
                        throw new InvalidOperationException(string.Format(ModelCreationExceptionMessage, customMap.Name));
                    }

                    instance.ConfigureMap(configurationExpression);
                }
            }

            var configuration = new MapperConfiguration(configurationExpression);

            return new Mapper(configuration);
        }
    }
}
