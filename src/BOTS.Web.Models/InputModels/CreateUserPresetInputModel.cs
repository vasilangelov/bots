namespace BOTS.Web.Models.InputModels
{
    using System.ComponentModel.DataAnnotations;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class CreateUserPresetInputModel : IMapFrom<UserPreset>, IMapTo<UserPreset>
    {
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Name")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "StringLength_MinMax")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Currency Pair")]
        public int CurrencyPairId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Chart Type")]
        public ChartType ChartType { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Payout")]
        [Range(0.5, double.PositiveInfinity, ErrorMessage = "Range")]
        public decimal Payout { get; set; }
    }
}
