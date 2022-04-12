namespace BOTS.Data.Models
{
    public class Nationality
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public virtual ICollection<ApplicationUser> Users { get; set; } = new HashSet<ApplicationUser>();
    }
}
