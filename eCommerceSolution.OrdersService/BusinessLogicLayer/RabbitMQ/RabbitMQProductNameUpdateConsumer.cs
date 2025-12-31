using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace BusinessLogicLayer.RabbitMQ //version 6.8.1
{
    internal class RabbitMQProductNameUpdateConsumer : IDisposable, IRabbitMQProductNameUpdateConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly IModel _channel;
        private readonly IConnection _connection;

        public RabbitMQProductNameUpdateConsumer(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ_HostName"],
                Port = int.Parse(_configuration["RabbitMQ_Port"]),
                UserName = _configuration["RabbitMQ_UserName"],
                Password = _configuration["RabbitMQ_Password"]
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Consume()
        {
            string routingKey = "product.update.name";
            string queueName = "orders.product.update.name.queue";
            string exchangeName = _configuration["RabbitMQ_Products_Exchange"];

            //tạo exchange nếu chưa tồn tại, nếu đã tồn tại thì mở exchange đó ra
            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Direct,
                durable: true);

            //tạo queue nếu chưa tồn tại, nếu đã tồn tại thì mở queue đó ra
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false, // true: cho phép nhiều connection cùng sử dụng queue này
                autoDelete: false, // true: khi không còn consumer nào kết nối đến queue này thì queue sẽ bị xóa
                arguments: null); // x-message-ttl, x-dead-letter-exchange, x-max-length, x-expiry...

            //binding: ràng buộc queue với exchange thông qua routing key
            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey);

        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
