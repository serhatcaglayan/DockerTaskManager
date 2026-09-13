using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IConnection _connection;
    private IModel _channel;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;

        // RabbitMQ'ya baðlan (Uygulama açýlýr açýlmaz)
        var factory = new ConnectionFactory() { HostName = "c_rabbitmq" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Ayný kuyruðu burada da tanýmlýyoruz ki, Publisher henüz göndermediyse bile hata almayalým
        _channel.QueueDeclare(queue: "task_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bildirim Servisi RabbitMQ'yu dinlemeye baþladý...");

        // Kuyruðu dinleyecek tüketiciyi (consumer) oluþtur
        var consumer = new EventingBasicConsumer(_channel);

        // Kuyruða mesaj düþtüðünde tetiklenecek olay
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Gerçek bir senaryoda burada Mail atýlýr, biz þimdilik logluyoruz
            _logger.LogInformation($"[x] RABBITMQ'DAN YAKALANDI: {message}");
        };

        // Tüketiciyi çalýþtýr
        _channel.BasicConsume(queue: "task_queue", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}