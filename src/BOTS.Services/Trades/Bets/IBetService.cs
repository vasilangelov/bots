namespace BOTS.Services.Trades.Bets
{
    public interface IBetService
    {
        Task PayoutBetsAsync(Guid tradingWindowId);

        Task<IEnumerable<T>> GetActiveBetsAsync<T>(Guid userId);

        // TODO: use input model???
        Task<T> PlaceBetAsync<T>(
            Guid userId,
            Guid bettingOptionId,
            BetType betType,
            decimal barrier,
            decimal payout);
    }
}
