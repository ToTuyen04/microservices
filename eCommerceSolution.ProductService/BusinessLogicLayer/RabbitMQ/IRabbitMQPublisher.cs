using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.RabbitMQ;
public interface IRabbitMQPublisher
{
    void Publish<T>(Dictionary<string, object> headers, T message);
}
