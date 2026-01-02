using BusinessLogicLayer.DTO;
using eCommerceSolution.OrdersService.BusinessLogicLayer.RabbitMQ;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.RabbitMQ //version 6.8.1
{
    internal class RabbitMQProductNameUpdateConsumer : IDisposable, IRabbitMQProductNameUpdateConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;
        private readonly IDistributedCache _cache;

        public RabbitMQProductNameUpdateConsumer(IConfiguration configuration, ILogger<RabbitMQProductNameUpdateConsumer> logger, IDistributedCache cache)
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
            //string routingKey = "product.update.*";
            Dictionary<string, object> headers = new Dictionary<string, object>(){
                {"x-match", "all" }, //all: tất cả các cặp key-value phải khớp; any: chỉ cần một trong các cặp key-value khớp
                {"event", "product.update" },
                {"rowCount", 1 }
            };
            string queueName = "orders.product.update.name.queue";
            string exchangeName = _configuration["RabbitMQ_Products_Exchange"];

            //tạo exchange nếu chưa tồn tại, nếu đã tồn tại thì mở exchange đó ra
            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Headers,
                durable: true);

            //tạo queue nếu chưa tồn tại, nếu đã tồn tại thì mở queue đó ra
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false, // false: cho phép nhiều connection cùng sử dụng queue này
                autoDelete: false, // true: khi không còn consumer nào kết nối đến queue này thì queue sẽ bị xóa
                arguments: null); // x-message-ttl, x-dead-letter-exchange, x-max-length, x-expiry...

            //binding: ràng buộc queue với exchange thông qua routing key
            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: string.Empty,
                arguments: headers);

            //received & consume message
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, args) =>
            {
                byte[] body = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                if (message != null)
                {
                    ProductDTO? productDTO = JsonSerializer.Deserialize<ProductDTO>(message);
                    _logger.LogInformation($"Product name updated: {productDTO.ProductID}, New name: {productDTO.ProductName}");
                    await HandleProductUpdation(productDTO);

                }
            };

            _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);

        }

        private async Task HandleProductUpdation(ProductDTO product)
        {
            string productString = JsonSerializer.Serialize(product);
            string cacheKeyToWrite = $"product:{product.ProductID}";
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            await _cache.SetStringAsync(cacheKeyToWrite, productString, options);
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
