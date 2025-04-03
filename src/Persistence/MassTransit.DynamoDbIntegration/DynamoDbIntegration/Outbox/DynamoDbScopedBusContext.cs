#nullable enable
namespace MassTransit.DynamoDbIntegration.Outbox
{
    using System;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2.DataModel;
    using Clients;
    using DependencyInjection;
    using DynamoDB.Transaction.Interfaces;
    using Middleware;
    using Middleware.Outbox;
    using Serialization;
    using Transports;


    public class DynamoDbScopedBusContext<TBus> :
        ScopedBusContext,
        OutboxSendContext
        where TBus : class, IBus
    {
        readonly TBus _bus;
        readonly IClientFactory _clientFactory;
        readonly IBusOutboxNotification _notification;
        readonly IServiceProvider _provider;
        readonly IDynamoDBContext _dynamoDbContext;
        readonly IDynamoDbScopedContext _dynamoDbScopedContext;
        readonly Guid _outboxId;
        IPublishEndpoint? _publishEndpoint;
        IScopedClientFactory? _scopedClientFactory;
        ISendEndpointProvider? _sendEndpointProvider;

        public DynamoDbScopedBusContext(TBus bus, IDynamoDBContext dynamoDbContext, IDynamoDbScopedContext dynamoDbScopedContext,
            IBusOutboxNotification notification, IClientFactory clientFactory, IServiceProvider provider)
        {
            _bus = bus;
            _notification = notification;
            _clientFactory = clientFactory;
            _provider = provider;
            _dynamoDbContext = dynamoDbContext;
            _dynamoDbScopedContext = dynamoDbScopedContext;

            _outboxId = NewId.NextGuid();
        }

        public Task AddSend<T>(SendContext<T> context)
            where T : class
        {
            var transactWrite = _dynamoDbContext.CreateTransactWrite<OutboxMessage>(new DynamoDBOperationConfig { OverrideTableName = "MassTransit-Outbox" });

            transactWrite.AddSend(context, SystemTextJsonMessageSerializer.Instance, outboxId: _outboxId);

            _dynamoDbScopedContext.AddTransactWrite(transactWrite);

            return Task.CompletedTask;
        }

        public object? GetService(Type serviceType)
        {
            return _provider.GetService(serviceType);
        }

        public ISendEndpointProvider SendEndpointProvider => _sendEndpointProvider ??= new OutboxSendEndpointProvider(this, GetSendEndpointProvider());

        public IPublishEndpoint PublishEndpoint =>
            _publishEndpoint ??= new PublishEndpoint(new OutboxPublishEndpointProvider(this, GetPublishEndpointProvider()));

        public IScopedClientFactory ClientFactory => _scopedClientFactory ??= GetClientFactory();

        protected virtual ScopedClientFactory GetClientFactory()
        {
            return new ScopedClientFactory(new ClientFactory(new ScopedClientFactoryContext(_clientFactory, _provider)), null);
        }

        protected virtual IPublishEndpointProvider GetPublishEndpointProvider()
        {
            return _bus;
        }

        protected virtual ISendEndpointProvider GetSendEndpointProvider()
        {
            return _bus;
        }
    }


    public class DynamoDbConsumeContextScopedBusContext<TBus> :
        DynamoDbScopedBusContext<TBus>
        where TBus : class, IBus
    {
        readonly TBus _bus;
        readonly IClientFactory _clientFactory;
        readonly ConsumeContext _consumeContext;
        readonly IServiceProvider _provider;

        public DynamoDbConsumeContextScopedBusContext(TBus bus, IDynamoDBContext dynamoDbContext, IDynamoDbScopedContext dynamoDbScopedContext,
            IBusOutboxNotification notification, IClientFactory clientFactory, IServiceProvider provider, ConsumeContext consumeContext)
            : base(bus, dynamoDbContext, dynamoDbScopedContext, notification, clientFactory, provider)
        {
            _bus = bus;
            _clientFactory = clientFactory;
            _provider = provider;
            _consumeContext = consumeContext;
        }

        protected override IPublishEndpointProvider GetPublishEndpointProvider()
        {
            return new ScopedConsumePublishEndpointProvider(_bus, _consumeContext, _provider);
        }

        protected override ISendEndpointProvider GetSendEndpointProvider()
        {
            return new ScopedConsumeSendEndpointProvider(_bus, _consumeContext, _provider);
        }

        protected override ScopedClientFactory GetClientFactory()
        {
            return new ScopedClientFactory(new ClientFactory(new ScopedClientFactoryContext(_clientFactory, _provider)), _consumeContext);
        }
    }
}
