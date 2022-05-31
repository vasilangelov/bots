namespace BOTS.Services.Trades.Bets
{
    public interface IBetService
    {
        Task PayoutBetsAsync(Guid tradingWindowId);

        Task<IEnumerable<T>> GetActiveUserBetsAsync<T>(Guid userId);

        Task<IEnumerable<T>> GetUserBetHistoryAsync<T>(Guid userId, int skip, int take);

        Task<int> GetUserHistoryPageCount(Guid userId, int itemsPerPage);

        // TODO: use input model???
        Task<T> PlaceBetAsync<T>(
            Guid userId,
            Guid bettingOptionId,
            BetType betType,
            decimal barrier,
            decimal payout);
    }
}
