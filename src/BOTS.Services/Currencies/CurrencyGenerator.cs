namespace BOTS.Services.Currencies
{
    using BOTS.Services.Infrastructure.Extensions;

    public static class CurrencyGenerator
    {
        public static decimal GenerateCurrencyRate(Random rnd, decimal value, decimal minOffset, decimal maxOffset)
        {
            int sign = rnd.Next(-1, 2);
            decimal delta = rnd.NextDecimal(maxOffset, minOffset);

            return value + sign * delta;
        }
    }
}
