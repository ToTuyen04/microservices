using BusinessLogicLayer.RabbitMQ;
using Microsoft.Extensions.Hosting;

namespace eCommerceSolution.OrdersService.BusinessLogicLayer.RabbitMQ;
public class RabbitMQProductNameUpdatedHostedService : IHostedService
{
    private readonly IRabbitMQProductNameUpdateConsumer _productNameUpdateConsumer;
    public RabbitMQProductNameUpdatedHostedService(IRabbitMQProductNameUpdateConsumer productNameUpdateConsumer)
    {
        _productNameUpdateConsumer = productNameUpdateConsumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _productNameUpdateConsumer.Consume();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productNameUpdateConsumer.Dispose();
        return Task.CompletedTask;
    }
}
