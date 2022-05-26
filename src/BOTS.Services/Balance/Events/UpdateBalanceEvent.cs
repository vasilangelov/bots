namespace BOTS.Services.Balance.Events
{
    public class UpdateBalanceEvent
    {
        public Guid UserId { get; set; }

        public decimal Balance { get; set; }
    }
}
