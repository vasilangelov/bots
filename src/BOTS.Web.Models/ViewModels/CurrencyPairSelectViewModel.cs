namespace BOTS.Web.Models.ViewModels
{
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc.Rendering;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class CurrencyPairSelectViewModel : SelectListItem, ICustomMap
    {
        private const string CurrencyPairDisplayFormat = "{0}/{1}";

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<CurrencyPair, CurrencyPairSelectViewModel>()
                .ForMember(x => x.Value, x => x.MapFrom(y => y.Id))
                .ForMember(x => x.Text, x => x.MapFrom(y => string.Format(CurrencyPairDisplayFormat, y.Left.Name, y.Right.Name)));
        }
    }
}
