namespace BOTS.Web.Models.InputModels
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System.ComponentModel.DataAnnotations;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class UpdateUserPresetInputModel : IMapFrom<UserPreset>, IMapTo<UserPreset>
    {
        [Required(ErrorMessage = "Required")]
        public Guid Id { get; set; } = default!;

        [BindNever]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "StringLength_MinMax")]
        [Display(Name = "Name")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Currency Pair")]
        public int CurrencyPairId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Chart Type")]
        public ChartType ChartType { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Payout")]
        [Range(0.5, double.MaxValue, ErrorMessage = "Range")]
        public decimal Payout { get; set; }
    }
}
