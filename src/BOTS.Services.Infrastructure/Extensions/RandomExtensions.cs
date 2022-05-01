namespace BOTS.Services.Infrastructure.Extensions
{
    public static class RandomExtensions
    {
        private const string ArgumentOutOfRangeExceptionMessage = "{0} cannot be less than {1}";

        public static int NextInt32(this Random random)
        {
            Span<byte> buffer = stackalloc byte[4];

            random.NextBytes(buffer);

            return BitConverter.ToInt32(buffer);
        }

        public static decimal NextDecimal(this Random random)
        {
            int lo = random.NextInt32();
            int mid = random.NextInt32();
            int hi = random.NextInt32();

            return new decimal(lo, mid, hi, false, 0) / decimal.MaxValue;
        }

        public static decimal NextDecimal(this Random random, decimal maxValue, decimal minValue = 0)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    ArgumentOutOfRangeExceptionMessage,
                    nameof(maxValue),
                    nameof(minValue)));
            }

            return random.NextDecimal() * (maxValue - minValue) + minValue;
        }
    }
}
