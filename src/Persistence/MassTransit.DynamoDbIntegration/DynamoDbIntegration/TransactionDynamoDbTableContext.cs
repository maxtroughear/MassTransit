#nullable enable
namespace MassTransit.DynamoDbIntegration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;


public class TransactionDynamoDbTableContext<T> : DynamoDbTableContext<T>
{
    readonly DynamoDbContext _context;
    readonly IAmazonDynamoDB _databaseClient;
    readonly IDynamoDBContext _databaseContext;
    readonly DynamoDbTableOptions<T> _options;

    public TransactionDynamoDbTableContext(DynamoDbContext context, IAmazonDynamoDB databaseClient, IDynamoDBContext databaseContext,
        IOptions<DynamoDbTableOptions<T>> options)
    {
        _context = context;
        _databaseClient = databaseClient;
        _databaseContext = databaseContext;
        _options = options.Value;
    }

    public async Task<List<T>> Query(QueryOperationConfig queryOperationConfig, CancellationToken cancellationToken)
    {
        return await _databaseContext.FromQueryAsync<T>(queryOperationConfig, _options.Config).GetRemainingAsync(cancellationToken);
    }

    public async Task<List<T>> Query(QueryRequest queryRequest, CancellationToken cancellationToken)
    {
        var response = await _databaseClient.QueryAsync(queryRequest, cancellationToken);

        return response.Items.Select(Document.FromAttributeMap).Select(_databaseContext.FromDocument<T>).ToList();
    }

    // public async Task<List<T>> Query(Expression keyExpression, int limit, bool consistentRead, string? index, CancellationToken cancellationToken)
    // {
    //     var queryOperationConfig = new QueryOperationConfig()
    //     {
    //         KeyExpression = keyExpression,
    //         Limit = limit,
    //         ConsistentRead = consistentRead,
    //         IndexName = index
    //     };
    //
    //     return await _databaseContext.FromQueryAsync<T>(queryOperationConfig, _options.Config).GetRemainingAsync(cancellationToken);
    // }

    public async Task Lock(UpdateItemRequest updateItemRequest, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _databaseClient.UpdateItemAsync(updateItemRequest, cancellationToken);
                return;
            }
            catch (ConditionalCheckFailedException)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken).ConfigureAwait(false);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        throw new OperationCanceledException("The document could not be locked");
    }

    public async Task Lock(T instance, CancellationToken cancellationToken)
    {
        _databaseContext.SaveAsync(instance, _options.Config, cancellationToken);
    }
}
