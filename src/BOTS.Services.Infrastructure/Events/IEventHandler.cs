namespace BOTS.Services.Infrastructure.Events
{
    public interface IEventHandler<TEvent>
    {
        Task InvokeAsync(TEvent context);
    }
}
