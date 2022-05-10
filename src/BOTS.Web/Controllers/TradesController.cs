namespace BOTS.Web.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using BOTS.Web.Models.ViewModels;
    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Services.Data.UserPresets;
    using BOTS.Web.Extensions;
    using BOTS.Web.Models.InputModels;

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
            var model = new LiveViewModel
            {
                CurrencyPairs = await this.currencyPairService
                                          .GetActiveCurrencyPairsAsync<CurrencyPairSelectViewModel>(),
            };

            return this.View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Presets(string? id)
        {
            var userId = this.User.GetUserId();

            if (id is null)
            {
                var presetId = await this.userPresetService.GetActiveUserPresetIdAsync(userId);

                if (presetId is null)
                {
                    return this.RedirectToAction(nameof(this.CreatePreset));
                }

                return this.RedirectToAction(nameof(this.Presets), new { Id = presetId });
            }

            bool userWithPresetExists = await this.userPresetService.IsUserPresetOwnerAsync(userId, id);

            if (!userWithPresetExists)
            {
                return this.RedirectToAction(nameof(this.Presets));
            }

            var model = new UserPresetsViewModel
            {
                CurrencyPairs = await this.currencyPairService
                                        .GetActiveCurrencyPairsAsync<CurrencyPairSelectViewModel>(),
                UserPresets = await this.userPresetService.GetUserPresetsAsync<UserPresetViewModel>(userId),
                CurrentPreset = await this.userPresetService.GetUserPresetAsync<UpdateUserPresetInputModel>(id),
            };

            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Presets([Bind(Prefix = nameof(UserPresetsViewModel.CurrentPreset))] UpdateUserPresetInputModel userPreset)
        {
            string userId = this.User.GetUserId();

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

            string userId = this.User.GetUserId();

            string id = await this.userPresetService.AddPresetAsync(userId, userPreset);

            return this.RedirectToAction(nameof(this.Presets), new { Id = id });
        }

        [HttpGet("/Presets/SetActive/:id")]
        public async Task<IActionResult> SetActivePreset(string id)
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
        public async Task<IActionResult> RemovePreset(string id)
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
