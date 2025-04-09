#nullable enable
namespace MassTransit.Configuration
{
    using System;
    using DynamoDbIntegration;
    using DynamoDbIntegration.Outbox;
    using MassTransit.DependencyInjection;
    using MassTransit.Middleware.Outbox;
    using Microsoft.Extensions.DependencyInjection;


    public class DynamoDbBusOutboxConfigurator :
        IDynamoDbBusOutboxConfigurator
    {
        readonly IBusRegistrationConfigurator _configurator;

        public DynamoDbBusOutboxConfigurator(IBusRegistrationConfigurator configurator)
        {
            _configurator = configurator;
        }

        /// <summary>
        /// No-op. This implementation requires using DynamoDB streams and a Lambda for publishing messages
        /// </summary>
        public void DisableDeliveryService()
        {
        }

        public virtual void Configure(Action<IDynamoDbBusOutboxConfigurator>? configure)
        {
            _configurator.ReplaceScoped<IScopedBusContextProvider<IBus>, DynamoDbScopedBusContextProvider<IBus>>();
            _configurator.AddSingleton<IBusOutboxNotification, BusOutboxNotification>();

            _configurator.AddOptions<OutboxDeliveryServiceOptions>()
                .Configure(options => { });

            configure?.Invoke(this);
        }
    }
}
