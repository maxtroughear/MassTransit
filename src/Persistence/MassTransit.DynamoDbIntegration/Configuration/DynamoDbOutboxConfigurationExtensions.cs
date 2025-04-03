#nullable enable
namespace MassTransit
{
    using System;
    using Configuration;


    public static class DynamoDbOutboxConfigurationExtensions
    {
        public static void AddDynamoDbBusOutbox(this IBusRegistrationConfigurator configurator, Action<IDynamoDbBusOutboxConfigurator>? configure = null)
        {
            var busOutboxConfigurator = new DynamoDbBusOutboxConfigurator(configurator);
            busOutboxConfigurator.Configure(configure);
        }
    }
}
