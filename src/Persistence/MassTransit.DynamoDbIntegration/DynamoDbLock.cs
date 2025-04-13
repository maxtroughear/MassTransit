namespace MassTransit;

using System;
using System.Threading.Tasks;


public class DynamoDbLock : IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}
