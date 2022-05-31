namespace BOTS.Services.Balance.User.Events
{
    public class UpdateBalanceEvent
    {
        public Guid UserId { get; set; }

        public decimal Balance { get; set; }
    }
}
