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
        // Baðlantý kodlarýný buradan kaldýrdýk çünkü uygulama açýlýr açýlmaz burasý çalýþýr.
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bildirim Servisi RabbitMQ'yu bekliyor...");

        // RabbitMQ'nun ayaða kalkmasý için Akýllý Yeniden Deneme (Retry) mekanizmasý
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "c_rabbitmq" };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: "task_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("RabbitMQ'ya BAÞARIYLA baðlanýldý!");
                break; // Baðlantý baþarýlý olursa döngüden çýk ve aþaðýdan devam et
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RabbitMQ henüz hazýr deðil. 5 saniye sonra tekrar denenecek...");
                await Task.Delay(5000, stoppingToken); // 5 saniye bekle ve tekrar dene
            }
        }

        // Tüketici (Consumer) ayarlarý
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"[x] RABBITMQ'DAN YAKALANDI: {message}");
        };

        _channel.BasicConsume(queue: "task_queue", autoAck: true, consumer: consumer);

        // Servisin kapanmamasý için sonsuz döngüde beklemesi gerekiyor
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        if (_channel != null) _channel.Close();
        if (_connection != null) _connection.Close();
        base.Dispose();
    }
}