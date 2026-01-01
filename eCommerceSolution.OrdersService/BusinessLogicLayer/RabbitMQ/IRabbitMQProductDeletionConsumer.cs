namespace eCommerceSolution.OrdersService.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQProductDeletionConsumer
    {
        void Consume();
        void Dispose();
    }
}