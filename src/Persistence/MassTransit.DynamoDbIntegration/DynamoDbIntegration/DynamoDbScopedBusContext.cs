#nullable enable
namespace MassTransit.DynamoDbIntegration
{
    using System;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2.DataModel;
    using DynamoDB.Transaction.Interfaces;
    using Clients;
    using DependencyInjection;
    using Middleware;
    using MassTransit.Middleware.Outbox;
    using Serialization;
    using Transports;
    using Outbox;


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
            // TODO(Max): Support multiple transactions (see MongoDB and EF Core implementations

            // TODO(Max): Make table name configurable (with a default)
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
}
