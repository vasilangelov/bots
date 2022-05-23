namespace BOTS.Data.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ApplicationUser : IdentityUser<Guid>
    {
        [Column(TypeName = "money")]
        public decimal Balance { get; set; }

        public int NationalityId { get; set; }

        public virtual Nationality Nationality { get; set; } = default!;

        public virtual ICollection<Bet> Bets { get; set; }
            = new HashSet<Bet>();

        public virtual ICollection<UserPreset> Presets { get; set; }
            = new HashSet<UserPreset>();
    }
}
