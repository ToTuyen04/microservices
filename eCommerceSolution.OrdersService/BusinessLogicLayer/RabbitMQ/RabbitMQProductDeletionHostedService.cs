using eCommerceSolution.OrdersService.BusinessLogicLayer.RabbitMQ;
using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ;
public class RabbitMQProductDeletionHostedService : IHostedService
{
    private readonly IRabbitMQProductDeletionConsumer _productDeletionConsumer;
    public RabbitMQProductDeletionHostedService(IRabbitMQProductDeletionConsumer productDeletionConsumer)
    {
        _productDeletionConsumer = productDeletionConsumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _productDeletionConsumer.Consume();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productDeletionConsumer.Dispose();
        return Task.CompletedTask;
    }
}
