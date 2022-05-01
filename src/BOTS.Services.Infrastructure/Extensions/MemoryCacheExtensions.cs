namespace BOTS.Services.Infrastructure.Extensions
{
    using Microsoft.Extensions.Caching.Memory;

    public static class MemoryCacheExtensions
    {
        private static class TypeLock<T>
        {
            public static object Lock { get; } = new();
        }

        public static T GetOrAdd<T>(this IMemoryCache memoryCache, object key, Func<T> factory)
        {
            if (memoryCache.TryGetValue<T>(key, out var outValue))
            {
                return outValue;
            }

            lock (TypeLock<T>.Lock)
            {
                if (memoryCache.TryGetValue(key, out outValue))
                {
                    return outValue!;
                }

                outValue = factory();

                memoryCache.Set(key, outValue);

                return outValue;
            }
        }
    }
}
