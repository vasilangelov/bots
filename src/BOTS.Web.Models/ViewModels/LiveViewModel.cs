﻿namespace BOTS.Web.Models.ViewModels
{
    public class LiveViewModel
    {
        public IEnumerable<CurrencyPairSelectViewModel> CurrencyPairs { get; set; } = default!;
    }
}
