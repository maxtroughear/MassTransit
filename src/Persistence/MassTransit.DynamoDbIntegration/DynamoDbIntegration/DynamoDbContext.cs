#nullable enable
namespace MassTransit.DynamoDbIntegration;

using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;


public interface DynamoDbContext
{
    MultiTableTransactWrite? MultiTableTransactWrite { get; }

    Guid? TransactionId { get; }

    void BeginTransaction();
    Task CommitTransaction(CancellationToken cancellationToken);
    void AbortTransaction();

    DynamoDbTableContext<T> GetTableContext<T>();
}
