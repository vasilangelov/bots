using BOTS.Services.Common.Results;

namespace BOTS.Services.Trades.Bets
{
    public interface IBetService
    {
        Task PayoutBetsAsync(Guid tradingWindowId);

        Task<T> GetBetAsync<T>(Guid betId);

        Task<IEnumerable<T>> GetActiveUserBetsAsync<T>(Guid userId);

        Task<IEnumerable<T>> GetUserBetHistoryAsync<T>(Guid userId, int skip, int take);

        Task<int> GetUserHistoryPageCount(Guid userId, int itemsPerPage);

        Task<Result<Guid>> PlaceBetAsync(
            Guid? userId,
            Guid bettingOptionId,
            BetType betType,
            decimal barrier,
            decimal payout);
    }
}
