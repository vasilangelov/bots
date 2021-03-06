namespace BOTS.Common
{
    public static class GlobalConstants
    {
        public const int CurrencyRateUpdateFrequency = 1 * 1000;

        public const int CurrencyRateStatUpdateFrequency = 3 * 1000;

        public const int CurrencyValueUpdateFrequency = 3 * 1000;

        public const int TradingWindowUpdateFrequency = 3 * 1000;

        public const int DisplayPastMinutesBetValues = 5;

        public const byte BarrierDigitPrecision = 18;

        public const byte DecimalPlacePrecision = 6;

        public const decimal MinCurrencyRateOffset = 0;

        public const decimal MaxCurrencyRateOffset = 0.000500m;
    }
}
