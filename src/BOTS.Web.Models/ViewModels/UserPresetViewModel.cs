namespace BOTS.Web.Models.ViewModels
{
    using BOTS.Services.Mapping;
    using BOTS.Data.Models;

    public class UserPresetViewModel : IMapFrom<UserPreset>
    {
        public string Id { get; set; } = default!;

        public string Name { get; set; } = default!;
    }
}
