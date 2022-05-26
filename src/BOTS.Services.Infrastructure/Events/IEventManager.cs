namespace BOTS.Services.Infrastructure.Events
{
    public interface IEventManager<TEvent>
    {
        void Subscribe<THandler>() where THandler : IEventHandler<TEvent>;

        Task EmitAsync(TEvent context);
    }
}
