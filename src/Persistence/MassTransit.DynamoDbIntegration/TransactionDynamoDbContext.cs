#nullable enable
namespace MassTransit;

using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using DynamoDbIntegration;


public class TransactionDynamoDbContext : DynamoDbContext
{
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
}
