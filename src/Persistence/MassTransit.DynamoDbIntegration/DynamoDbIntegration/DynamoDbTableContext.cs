#nullable enable
namespace MassTransit.DynamoDbIntegration;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;


public interface DynamoDbTableContext<T>
{
    Task<List<T>> Query(QueryRequest queryRequest, CancellationToken cancellationToken);
    Task<List<T>> Query(object hashKeyValue, CancellationToken cancellationToken);
    Task<List<T>> Query(object hashKeyValue, QueryOperator queryOperator, IEnumerable<object> rangeKeyValues, CancellationToken cancellationToken);
    Task Lock(object lockId, CancellationToken cancellationToken);
}
