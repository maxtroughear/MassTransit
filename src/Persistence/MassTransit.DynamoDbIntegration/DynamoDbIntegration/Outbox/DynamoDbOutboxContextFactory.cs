namespace MassTransit.DynamoDbIntegration.Outbox;

using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Middleware;


public class DynamoDbOutboxContextFactory :
    IOutboxContextFactory<DynamoDbContext>
{
    readonly DynamoDbContext _dbContext;
    //readonly ILockProvider _lockProvider;

    public DynamoDbOutboxContextFactory(DynamoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Send<T>(ConsumeContext<T> context, OutboxConsumeOptions options, IPipe<OutboxConsumeContext<T>> next)
        where T : class
    {
        var updateReceiveCount = true;

        var messageId = context.GetOriginalMessageId() ?? throw new MessageException(typeof(T), "MessageId required to use the outbox");



        async Task<bool> Execute()
        {
            _dbContext.BeginTransaction();

            try
            {
                var inboxStateTable = _dbContext.GetTableContext<InboxState>();

                inboxStateTable.Query()

                bool continueProcessing;


                await _dbContext.CommitTransaction(context.CancellationToken).ConfigureAwait(false);

                return continueProcessing;
            }
            catch (Exception)
            {
                _dbContext.AbortTransaction();
                //_lockProvider.Release(lockId);
                //lock.Release(); // disposable?
                throw;
            }
        }

        var continueProcessing = true;
        while (continueProcessing)
            continueProcessing = await Execute().ConfigureAwait(false);
    }

    public void Probe(ProbeContext context)
    {
        var scope = context.CreateFilterScope("outboxContextFactory");
        scope.Add("provider", "dynamoDb");
    }
}
