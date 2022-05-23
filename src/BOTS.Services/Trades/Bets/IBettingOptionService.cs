namespace BOTS.Services.Trades.Bets
{
    public interface IBettingOptionService
    {
        Task BulkCreateBettingOptionsAsync(
            Guid tradingWindowId,
            IEnumerable<BettingOptionPreset> bettingOptionPresets);

        Task SetBettingOptionsClosingValueAsync(Guid tradingWindowId, DateTime windowEnd);

        Task<bool> IsBettingOptionActiveAsync(Guid bettingOptionId);

        Task<DateTime> GetBettingOptionEndAsync(Guid bettingOptionId);

        Task<IEnumerable<T>> GetAllActiveBettingOptionsAsync<T>();

        Task<IEnumerable<T>> GetActiveBettingOptionsForCurrencyPairAsync<T>(int currencyPairId);

        Task<T> GetBettingOptionAsync<T>(Guid bettingOptionId);
    }
}
