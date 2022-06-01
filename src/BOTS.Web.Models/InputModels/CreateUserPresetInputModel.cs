﻿namespace BOTS.Web.Models.InputModels
{
    using System.ComponentModel.DataAnnotations;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class CreateUserPresetInputModel : IMapFrom<UserPreset>, IMapTo<UserPreset>
    {
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
        [Range(0.5, double.PositiveInfinity, ErrorMessage = "The field {0} must be greater than {1}")]
        public decimal Payout { get; set; }
    }
}
