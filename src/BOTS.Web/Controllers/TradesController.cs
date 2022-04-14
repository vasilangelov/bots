namespace BOTS.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using BOTS.Services;
    using BOTS.Data.Models;
    using BOTS.Web.Models;

    [Authorize]
    public class TradesController : Controller
    {
        private readonly IRepository<CurrencyPair> currencyPairRepository;

        public TradesController(IRepository<CurrencyPair> currencyPairRepository)
        {
            this.currencyPairRepository = currencyPairRepository;
        }

        public IActionResult Live()
        {
            var model = new LiveViewModel
            {
                CurrencyPairs = this.currencyPairRepository.AllAsNotracking().Where(x => x.Display).Select(x => new CurrencyPairViewModel
                {
                    LeftName = x.Left.Name,
                    RightName = x.Right.Name
                }).ToArray(),
            };

            return this.View(model);
        }
    }
}
