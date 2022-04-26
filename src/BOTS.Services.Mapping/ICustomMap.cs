namespace BOTS.Services.Mapping
{
    using AutoMapper;

    public interface ICustomMap
    {
        void ConfigureMap(IProfileExpression configuration);
    }
}
