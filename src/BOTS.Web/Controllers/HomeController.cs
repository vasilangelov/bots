namespace BOTS.Web.Controllers
{
    using System.Diagnostics;
    using Microsoft.AspNetCore.Mvc;

    using BOTS.Services;
    using Models;

    public class HomeController : Controller
    {
        private readonly ICurrencyProviderService currencyProviderService;

        public HomeController(ICurrencyProviderService currencyProviderService)
        {
            this.currencyProviderService = currencyProviderService;
        }

        public IActionResult Index()
        {
            var model = this.currencyProviderService.GetCurrencyInfo();

            return this.View(model);
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}