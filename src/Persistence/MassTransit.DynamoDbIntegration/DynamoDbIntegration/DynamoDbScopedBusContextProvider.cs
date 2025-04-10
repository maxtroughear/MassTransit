namespace MassTransit.DynamoDbIntegration
{
    using System;
    using Amazon.DynamoDBv2.DataModel;
    using DependencyInjection;
    using MassTransit.Middleware.Outbox;


    public class DynamoDbScopedBusContextProvider<TBus> :
        IScopedBusContextProvider<TBus>
        where TBus : class, IBus
    {
        public ScopedBusContext Context { get; }

        public DynamoDbScopedBusContextProvider(TBus bus, DynamoDbContext dbContext,
            IBusOutboxNotification notification,
            Bind<TBus, IClientFactory> clientFactory, Bind<TBus, IScopedConsumeContextProvider> consumeContextProvider,
            IScopedConsumeContextProvider globalConsumeContextProvider, IServiceProvider provider)
        {
            if (consumeContextProvider.Value.HasContext)
                Context = new ConsumeContextScopedBusContext(consumeContextProvider.Value.GetContext(), clientFactory.Value);
            else if (globalConsumeContextProvider.HasContext)
            {
                Context = new DynamoDbConsumeContextScopedBusContext<TBus>(bus, dbContext, notification, clientFactory.Value,
                    provider,
                    globalConsumeContextProvider.GetContext());
            }
            else
                Context = new DynamoDbScopedBusContext<TBus>(bus, dbContext, notification, clientFactory.Value, provider);
        }
    }
}
