using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

Console.WriteLine("🎧 RabbitMQ Consumer çalışıyor...");
// Bağlantı oluştur
var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5673 // Docker dış portu
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Dinlemek istediğin kuyruk ismini yaz (API'dekiyle aynı olmalı)
string queueName = "cart-queue";

channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var json = Encoding.UTF8.GetString(body);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n📩 Yeni mesaj alındı: {json}");

    // İstersen deserialize edip ayrı ayrı yazdır:
    var data = JsonSerializer.Deserialize<CartMessage>(json);
    Console.WriteLine($"👤 Kullanıcı: {data?.UserId} | 🛒 Ürün: {data?.ProductName} | Adet: {data?.Quantity}");

    Console.ResetColor();
};

channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine("🔁 Mesajlar dinleniyor. Çıkmak için Ctrl+C bas...");
Console.ReadLine();

record CartMessage(string UserId, string ProductId, string ProductName, int Quantity);

