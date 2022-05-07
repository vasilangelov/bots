namespace BOTS.Services.Data.Bets
{
    public interface IBetService
    {
        Task<IEnumerable<T>> GetActiveBetsAsync<T>(string userId);

        Task<T> PlaceBetAsync<T>(
            string userId,
            BetType betType,
            string tradingWindowId,
            byte barrierIndex,
            decimal payout);
    }
}
