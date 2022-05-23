namespace BOTS.Web.Models.ViewModels
{
    using AutoMapper;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    using Microsoft.AspNetCore.Mvc.Rendering;

    public class CurrencyPairSelectViewModel : SelectListItem, ICustomMap
    {
        private const string CurrencyPairDisplayFormat = "{0}/{1}";

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<CurrencyPair, CurrencyPairSelectViewModel>()
                .ForMember(x => x.Value, x => x.MapFrom(y => y.Id))
                .ForMember(x => x.Text, x => x.MapFrom(y => string.Format(CurrencyPairDisplayFormat, y.CurrencyFrom.Name, y.CurrencyTo.Name)));
        }
    }
}
