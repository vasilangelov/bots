namespace BOTS.Web.Models.ViewModels
{
    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class NationalityViewModel : IMapFrom<Nationality>
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;
    }
}
