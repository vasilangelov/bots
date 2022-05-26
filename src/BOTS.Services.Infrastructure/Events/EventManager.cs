namespace BOTS.Services.Infrastructure.Events
{
    using Microsoft.Extensions.DependencyInjection;

    public class EventManager<TEvent> : IEventManager<TEvent>
    {
        private readonly ICollection<Type> eventHandlers;
        private readonly IServiceProvider serviceProvider;

        public EventManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            this.eventHandlers = new HashSet<Type>();
        }

        public async Task EmitAsync(TEvent context)
        {
            using var scope = this.serviceProvider.CreateScope();

            foreach (var handler in this.eventHandlers)
            {
                var handlerInstance = ActivatorUtilities.CreateInstance(this.serviceProvider, handler) as IEventHandler<TEvent>;

                if (handlerInstance is null)
                {
                    throw new InvalidOperationException(string.Format("Handler {0} does not implement {1}", handler.Name, nameof(IEventHandler<TEvent>)));
                }

                await handlerInstance.InvokeAsync(context);
            }
        }

        public void Subscribe<THandler>() where THandler : IEventHandler<TEvent>
            => this.eventHandlers.Add(typeof(THandler));
    }
}
