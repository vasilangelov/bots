namespace BOTS.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class Treasury
    {
        public int Id { get; set; }

        [Column(TypeName = "money")]
        public decimal SystemBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal UserProfits { get; set; }
    }
}
