using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastCart.Application.Interfaces
{
    public interface IRabbitMqService
    {
        void Publish(string queueName, object message);
    }

}
