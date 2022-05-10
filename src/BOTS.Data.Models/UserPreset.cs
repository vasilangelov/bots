namespace BOTS.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class UserPreset
    {
        public UserPreset()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public string Name { get; set; } = default!;

        public string OwnerId { get; set; } = default!;

        public virtual ApplicationUser Owner { get; set; } = default!;

        public bool IsActive { get; set; }

        public int CurrencyPairId { get; set; }

        public virtual CurrencyPair CurrencyPair { get; set; } = default!;

        public virtual ChartType ChartType { get; set; } = default!;

        [Column(TypeName = "money")]
        public decimal Payout { get; set; }
    }
}
