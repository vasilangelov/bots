namespace BOTS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ApplicationSetting
    {
        public int Id { get; set; }

        public string Key { get; set; } = default!;

        [MaxLength(1024)]
        public string Value { get; set; } = default!;
    }
}
