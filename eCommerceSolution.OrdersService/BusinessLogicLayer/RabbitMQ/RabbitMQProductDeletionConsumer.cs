using BusinessLogicLayer.RabbitMQ;
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
    public RabbitMQProductDeletionConsumer(IConfiguration configuration, ILogger<RabbitMQProductDeletionConsumer> logger)
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
    }

    public void Consume()
    {
        string queueName = "orders.product.delete.queue";
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"];
        string routingKey = "product.delete";
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout, durable: true);

        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: null);

        //received & consume message
        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            if (message != null)
            {
                ProductDeletionMessage? productDeletionMessage = JsonSerializer.Deserialize<ProductDeletionMessage>(message);
                _logger.LogInformation($"Product deleted: {productDeletionMessage.ProductID}, Name: {productDeletionMessage.ProductName}");

            }
        };
        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}

