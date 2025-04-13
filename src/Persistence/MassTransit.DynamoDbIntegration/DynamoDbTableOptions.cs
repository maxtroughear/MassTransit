namespace MassTransit;

using Amazon.DynamoDBv2.DataModel;


public class DynamoDbTableOptions<T>
{
    public string TableName { get; set; }
}
