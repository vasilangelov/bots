namespace BOTS.Web.Models.ViewModels
{
    using BOTS.Web.Models.InputModels;

    public class UserPresetsViewModel
    {
        public IEnumerable<UserPresetViewModel> UserPresets { get; set; } = default!;

        public IEnumerable<CurrencyPairSelectViewModel> CurrencyPairs { get; set; } = default!;

        public UpdateUserPresetInputModel CurrentPreset { get; set; } = default!;
    }
}
