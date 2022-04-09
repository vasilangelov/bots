namespace BOTS.Data.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ApplicationUser : IdentityUser
    {
        [Column(TypeName = "money")]
        public decimal Balance { get; set; }
    }
}
