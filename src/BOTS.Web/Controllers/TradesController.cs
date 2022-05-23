namespace BOTS.Web.Controllers
{
    using BOTS.Services.Currencies.CurrencyRates;
    using BOTS.Services.UserPresets;
    using BOTS.Web.Extensions;
    using BOTS.Web.Models.InputModels;
    using BOTS.Web.Models.ViewModels;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class TradesController : Controller
    {
        private readonly ICurrencyPairService currencyPairService;
        private readonly IUserPresetService userPresetService;

        public TradesController(
            ICurrencyPairService currencyPairService,
            IUserPresetService userPresetService)
        {
            this.currencyPairService = currencyPairService;
            this.userPresetService = userPresetService;
        }

        public async Task<IActionResult> Live()
        {
            var userId = this.User.GetUserId();

            var model = new LiveViewModel
            {
                CurrencyPairs = await this.currencyPairService
                                          .GetActiveCurrencyPairsAsync<CurrencyPairSelectViewModel>(),
                Preset = await this.userPresetService.GetActiveUserPresetOrDefaultAsync<DisplayUserPresetViewModel>(userId)
            };

            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Presets(Guid? id)
        {
            var userId = this.User.GetUserId();

            if (!id.HasValue)
            {
                var presetId = await this.userPresetService.GetActiveUserPresetIdAsync(userId);

                if (presetId is null)
                {
                    return this.RedirectToAction(nameof(this.CreatePreset));
                }

                return this.RedirectToAction(nameof(this.Presets), new { Id = presetId });
            }

            bool userWithPresetExists = await this.userPresetService.IsUserPresetOwnerAsync(userId, id.Value);

            if (!userWithPresetExists)
            {
                return this.RedirectToAction(nameof(this.Presets));
            }

            var model = new UserPresetsViewModel
            {
                CurrencyPairs = await this.currencyPairService
                                        .GetActiveCurrencyPairsAsync<CurrencyPairSelectViewModel>(),
                UserPresets = await this.userPresetService.GetUserPresetsAsync<UserPresetViewModel>(userId),
                CurrentPreset = await this.userPresetService.GetUserPresetAsync<UpdateUserPresetInputModel>(id.Value),
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Presets([Bind(Prefix = nameof(UserPresetsViewModel.CurrentPreset))] UpdateUserPresetInputModel userPreset)
        {
            var userId = this.User.GetUserId();

            bool userWithPresetExists = await this.userPresetService.IsUserPresetOwnerAsync(userId, userPreset.Id);

            if (!this.ModelState.IsValid || !userWithPresetExists)
            {
                var model = new UserPresetsViewModel
                {
                    CurrencyPairs = await this.currencyPairService
                                            .GetActiveCurrencyPairsAsync<CurrencyPairSelectViewModel>(),
                    UserPresets = await this.userPresetService.GetUserPresetsAsync<UserPresetViewModel>(userId),
                    CurrentPreset = await this.userPresetService.GetActiveUserPresetAsync<UpdateUserPresetInputModel>(userId),
                };

                if (!userWithPresetExists)
                {
                    this.ModelState.AddModelError(string.Empty, "You don't own this preset");
                }

                return this.View(model);
            }

            await this.userPresetService.UpdatePresetAsync(userPreset);

            return this.RedirectToAction(nameof(this.Presets), new { userPreset.Id });
        }

        [HttpGet("/Presets/Create")]
        public async Task<IActionResult> CreatePreset()
        {
            var model = new CreateUserPresetViewModel
            {
                CurrencyPairs = await this.currencyPairService.GetActiveCurrencyPairsAsync<CurrencyPairSelectViewModel>(),
            };

            return this.View(model);
        }

        [HttpPost("/Presets/Create")]
        public async Task<IActionResult> CreatePreset([Bind(Prefix = nameof(CreateUserPresetViewModel.Preset))] CreateUserPresetInputModel userPreset)
        {
            if (!this.ModelState.IsValid)
            {
                var model = new CreateUserPresetViewModel
                {
                    CurrencyPairs = await this.currencyPairService.GetActiveCurrencyPairsAsync<CurrencyPairSelectViewModel>(),
                    Preset = userPreset,
                };

                return this.View(model);
            }

            var userId = this.User.GetUserId();

            var id = await this.userPresetService.AddPresetAsync(userId, userPreset);

            return this.RedirectToAction(nameof(this.Presets), new { Id = id });
        }

        [HttpGet("/Presets/SetActive/:id")]
        public async Task<IActionResult> SetActivePreset(Guid id)
        {
            var userId = this.User.GetUserId();

            bool userWithPresetExists = await this.userPresetService.IsUserPresetOwnerAsync(userId, id);

            if (!userWithPresetExists)
            {
                this.TempData["Error"] = "Invalid preset";

                return this.RedirectToAction(nameof(this.Presets));
            }

            await this.userPresetService.SetDefaultPresetAsync(userId, id);

            return this.RedirectToAction(nameof(this.Presets), new { Id = id });
        }

        [HttpGet("/Presets/Delete/:id")]
        public async Task<IActionResult> RemovePreset(Guid id)
        {
            var userId = this.User.GetUserId();

            bool userWithPresetExists = await this.userPresetService.IsUserPresetOwnerAsync(userId, id);

            if (!userWithPresetExists)
            {
                this.TempData["Error"] = "Invalid preset";

                return this.RedirectToAction(nameof(this.Presets));
            }

            await this.userPresetService.DeletePresetAsync(id);

            return this.RedirectToAction(nameof(this.Presets));
        }
    }
}
