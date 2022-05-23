namespace BOTS.Services.Trades.Bets
{
    public delegate decimal BarrierEntryPercentageFormula(
        decimal currencyRate,
        decimal barrier,
        decimal delta,
        long remaining,
        long fullTime);
}
