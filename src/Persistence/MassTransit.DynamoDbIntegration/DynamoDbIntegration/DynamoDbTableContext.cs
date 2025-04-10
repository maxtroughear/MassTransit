#nullable enable
namespace MassTransit.DynamoDbIntegration;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;


public interface DynamoDbTableContext<T>
{
    Task<List<T>> Query(QueryRequest queryRequest, CancellationToken cancellationToken);

    Task Lock(UpdateItemRequest updateItemRequest, CancellationToken cancellationToken);
}
