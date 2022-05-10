namespace BOTS.Web.Models.InputModels
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System.ComponentModel.DataAnnotations;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class UpdateUserPresetInputModel : IMapFrom<UserPreset>, IMapTo<UserPreset>
    {
        [Required]
        public string Id { get; set; } = default!;

        [BindNever]
        public bool IsActive { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 5)]
        public string Name { get; set; } = default!;

        [Required]
        [Display(Name = "Currency Pair")]
        public int CurrencyPairId { get; set; }

        [Required]
        [Display(Name = "Chart Type")]
        public ChartType ChartType { get; set; }

        [Required]
        [Range(0.5, double.MaxValue, ErrorMessage = "The field {0} must be greater than {1}")]
        public decimal Payout { get; set; }
    }
}
