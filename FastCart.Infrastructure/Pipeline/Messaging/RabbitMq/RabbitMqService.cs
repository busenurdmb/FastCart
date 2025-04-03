
using FastCart.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class RabbitMqService : IRabbitMqService
{
    private readonly IConnection _connection;

    public RabbitMqService()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5673 // Docker dış portu (docker-compose.yml'deki ayara göre)
        };
        _connection = factory.CreateConnection();
    }

    public void Publish(string queueName, object message)
    {
        using var channel = _connection.CreateModel();
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }
}
