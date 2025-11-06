using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserManagement.Services.Events
{
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent evt) where TEvent : IUserDomainEvent; //raise event
        void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IUserDomainEvent; //map methods to events
    }

    public class InMemoryEventBus : IEventBus
    {
        //all subscriptions
        private readonly Dictionary<Type, List<Func<IUserDomainEvent, Task>>> _handlers = new();


        public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IUserDomainEvent
        {
            var type = typeof(TEvent);//type of the event

            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Func<IUserDomainEvent, Task>>();

            _handlers[type].Add(evt => handler((TEvent)evt));
        }

        public async Task PublishAsync<TEvent>(TEvent evt) where TEvent : IUserDomainEvent
        {
            var type = typeof(TEvent);
            if (!_handlers.ContainsKey(type)) return;

            foreach (var handler in _handlers[type])
                await handler(evt);
        }
    }
}
