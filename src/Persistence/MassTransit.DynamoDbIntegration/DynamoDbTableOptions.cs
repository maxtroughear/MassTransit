namespace MassTransit;

using Amazon.DynamoDBv2.DataModel;


public class DynamoDbTableOptions<T>
{
    public DynamoDBOperationConfig Config { get; set; }
}
