﻿namespace BOTS.Services.Data.Users
{
    public interface IUserService
    {
        Task<decimal> GetUserBalance(string userId);

        Task<bool> SubtractFromBalanceAsync(string userId, decimal amount);

        Task<bool> HasActiveBetForCurrencyPairAsync(string userId, int currencyPairId);
    }
}
