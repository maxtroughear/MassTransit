#nullable enable
namespace MassTransit
{
    using System;


    public interface IDynamoDbOutboxConfigurator :
        ITransactionalOutboxConfigurator
    {
        /// <summary>
        /// The Bus Outbox intercepts the <see cref="ISendEndpointProvider" /> and <see cref="IPublishEndpoint" /> interfaces
        /// that are used when not consuming messages. Messages sent or published via those interfaces are written to the outbox
        /// instead of being delivered directly to the message broker.
        /// </summary>
        void UseBusOutbox(Action<IDynamoDbBusOutboxConfigurator>? configure = null);
    }
}
