#nullable enable
namespace MassTransit.Configuration
{
    using System;


    public class DynamoDbOutboxConfigurator :
        IDynamoDbOutboxConfigurator
    {
        readonly IBusRegistrationConfigurator _configurator;

        public DynamoDbOutboxConfigurator(IBusRegistrationConfigurator configurator)
        {
            _configurator = configurator;
        }

        public virtual void UseBusOutbox(Action<IDynamoDbBusOutboxConfigurator>? configure = null)
        {
            var busOutboxConfigurator = new DynamoDbBusOutboxConfigurator(_configurator);
            busOutboxConfigurator.Configure(configure);
        }

        public virtual void Configure(Action<IDynamoDbOutboxConfigurator>? configure)
        {
        }
    }
}
