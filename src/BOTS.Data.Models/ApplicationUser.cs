namespace BOTS.Data.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ApplicationUser : IdentityUser
    {
        [Column(TypeName = "money")]
        public decimal Balance { get; set; }

        public int NationalityId { get; set; }

        public virtual Nationality Nationality { get; set; } = default!;

        public virtual IEnumerable<Bet> Bets { get; set; } = default!;
    }
}
