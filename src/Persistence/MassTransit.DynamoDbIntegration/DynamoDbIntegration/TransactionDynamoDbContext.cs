#nullable enable
namespace MassTransit.DynamoDbIntegration;

using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


public class TransactionDynamoDbContext : DynamoDbContext
{
    readonly IServiceProvider _serviceProvider;

    public TransactionDynamoDbContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public MultiTableTransactWrite? MultiTableTransactWrite { get; private set; }

    public Guid? TransactionId { get; private set; }

    public void BeginTransaction()
    {
        if (MultiTableTransactWrite is null)
        {
            MultiTableTransactWrite = new MultiTableTransactWrite();
            TransactionId = NewId.NextGuid();
        }
    }

    public async Task CommitTransaction(CancellationToken cancellationToken)
    {
        if (MultiTableTransactWrite is null)
            throw new InvalidOperationException("No MultiTableTransactWrite has been created");

        await MultiTableTransactWrite.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        TransactionId = null;
        MultiTableTransactWrite = null;
    }

    public void AbortTransaction()
    {
        if (MultiTableTransactWrite is null)
            throw new InvalidOperationException("No MultiTableTransactWrite has been created");

        TransactionId = null;
        MultiTableTransactWrite = null;
    }

    public DynamoDbTableContext<T> GetTableContext<T>()
    {
        var dynamoDbClient = _serviceProvider.GetRequiredService<IAmazonDynamoDB>();
        var dynamoDbContext = _serviceProvider.GetRequiredService<IDynamoDBContext>();
        var options = _serviceProvider.GetRequiredService<IOptions<DynamoDbTableOptions<T>>>();


        return new TransactionDynamoDbTableContext<T>(this, dynamoDbClient, dynamoDbContext, options);
    }
}
