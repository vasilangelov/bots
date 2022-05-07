namespace BOTS.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using BOTS.Web.Models.ViewModels;
    using BOTS.Services.Data.CurrencyPairs;

    [Authorize]
    public class TradesController : Controller
    {
        private readonly ICurrencyPairService currencyPairService;

        public TradesController(ICurrencyPairService currencyPairService)
        {
            this.currencyPairService = currencyPairService;
        }

        public async Task<IActionResult> Live()
        {
            var model = new LiveViewModel
            {
                CurrencyPairs = await this.currencyPairService
                                          .GetActiveCurrencyPairsAsync<CurrencyPairViewModel>(),
            };

            return this.View(model);
        }
    }
}
