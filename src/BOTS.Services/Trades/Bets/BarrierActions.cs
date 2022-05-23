namespace BOTS.Services.Trades.Bets
{
    using static BOTS.Common.GlobalConstants;

    public static class BarrierActions
    {
        public static decimal GetEntryPercentage(
            decimal[] barriers,
            decimal barrier,
            decimal barrierStep,
            decimal currencyRate,
            long remainingTime,
            long fullTime,
            BetType betType)
        {
            BarrierEntryPercentageFormula barrierPercentageFormula = betType switch
            {
                BetType.Higher => GetHigherPercentage,
                BetType.Lower => GetLowerPercentage,
                _ => throw new ArgumentException(string.Format("Invalid {0}", nameof(BetType)), nameof(betType))
            };

            decimal delta = barriers.Length * (barrierStep == 0 ? 1 : barrierStep);

            return barrierPercentageFormula(currencyRate, barrier, delta, remainingTime, fullTime);
        }

        internal static decimal[] GenerateBarriers(
            byte barrierCount,
            decimal openingPrice,
            decimal barrierStep)
        {
            int startingIndex = -barrierCount / 2;

            return Enumerable.Range(startingIndex, barrierCount)
                             .Select(barrierIndex =>
                                 decimal.Round(
                                    openingPrice + barrierIndex * barrierStep,
                                    DecimalPlacePrecision))
                             .ToArray();
        }

        private static decimal GetHigherPercentage(
            decimal currencyRate,
            decimal barrier,
            decimal delta,
            long remaining,
            long fullTime)
            => (currencyRate - barrier) / delta + 0.5m * (2 - remaining / (decimal)fullTime);

        private static decimal GetLowerPercentage(
            decimal currencyRate,
            decimal barrier,
            decimal delta,
            long remaining,
            long fullTime)
           => (barrier - currencyRate) / delta + 0.5m * (2 - remaining / (decimal)fullTime);
    }
}
