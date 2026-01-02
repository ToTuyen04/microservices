using BusinessLogicLayer.RabbitMQ;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace eCommerceSolution.OrdersService.BusinessLogicLayer.RabbitMQ;
internal class RabbitMQProductDeletionConsumer : IDisposable, IRabbitMQProductDeletionConsumer
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQProductDeletionConsumer> _logger;
    private readonly IDistributedCache _cache;
    public RabbitMQProductDeletionConsumer(IConfiguration configuration, ILogger<RabbitMQProductDeletionConsumer> logger, IDistributedCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ_HostName"],
            Port = Convert.ToInt32(_configuration["RabbitMQ_Port"]),
            UserName = _configuration["RabbitMQ_UserName"],
            Password = _configuration["RabbitMQ_Password"]
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _cache = cache;
    }

    public void Consume()
    {
        string queueName = "orders.product.delete.queue";
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"];
        //string routingKey = "product.#";
        Dictionary<string, object> headers = new Dictionary<string, object>(){
            {"x-match", "all" },
            {"event", "product.delete" },
            {"rowCount", 1 }
        };
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

        //received & consume message
        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            if (message != null)
            {
                ProductDeletionMessage? productDeletionMessage = JsonSerializer.Deserialize<ProductDeletionMessage>(message);
                _logger.LogInformation($"Product deleted: {productDeletionMessage.ProductID}, Name: {productDeletionMessage.ProductName}");
                await HandleProductDeletion(productDeletionMessage.ProductID);

            }
        };
        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
    }

    private async Task HandleProductDeletion(Guid productID)
    {
        string cacheKeyToWrite = $"product:{productID}";

        await _cache.RemoveAsync(cacheKeyToWrite);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}

