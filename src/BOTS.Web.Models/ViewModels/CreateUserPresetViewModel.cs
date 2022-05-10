namespace BOTS.Web.Models.ViewModels
{
    using BOTS.Web.Models.InputModels;

    public class CreateUserPresetViewModel
    {
        public IEnumerable<CurrencyPairSelectViewModel> CurrencyPairs { get; set; } = default!;

        public CreateUserPresetInputModel Preset { get; set; } = default!;
    }
}
