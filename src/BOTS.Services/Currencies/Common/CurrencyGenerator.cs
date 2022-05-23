namespace BOTS.Services.Currencies.Common
{
    using BOTS.Services.Infrastructure.Extensions;

    internal static class CurrencyGenerator
    {
        public static decimal GenerateCurrencyRate(
            Random rnd,
            decimal value,
            decimal minOffset,
            decimal maxOffset)
        {
            int sign = rnd.Next(-1, 2);
            decimal delta = rnd.NextDecimal(maxOffset, minOffset);

            return value + sign * delta;
        }
    }
}
