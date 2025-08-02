using Confluent.Kafka;
using KafkaPOC.Core.Configuration;
using KafkaPOC.Core.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KafkaPOC.Producer;

public class KafkaProducerService : BackgroundService
{
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly KafkaSettings _kafkaSettings;
    private readonly IProducer<string, string> _producer;

    public KafkaProducerService(
        ILogger<KafkaProducerService> logger,
        IOptions<KafkaSettings> kafkaSettings)
    {
        _logger = logger;
        _kafkaSettings = kafkaSettings.Value;
        
        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            Acks = Acks.All,
            RetryBackoffMs = 1000,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Erro no Producer: {Error}", e.Reason))
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka Producer Service iniciado");

        var random = new Random();
        var customerIds = new[] { "CUST001", "CUST002", "CUST003", "CUST004" };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Gerar evento de pedido
                var orderEvent = new OrderEvent
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = customerIds[random.Next(customerIds.Length)],
                    Amount = random.Next(100, 1000),
                    CreatedAt = DateTime.UtcNow,
                    Status = "Created",
                    Items = GenerateRandomItems(random)
                };

                var message = JsonConvert.SerializeObject(orderEvent);
                
                var result = await _producer.ProduceAsync(
                    Topics.Orders,
                    new Message<string, string>
                    {
                        Key = orderEvent.CustomerId,
                        Value = message
                    },
                    stoppingToken);

                _logger.LogInformation(
                    "Mensagem enviada - Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, OrderId: {OrderId}",
                    result.Topic, result.Partition.Value, result.Offset.Value, orderEvent.OrderId);

                // Aguardar antes da próxima mensagem
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao produzir mensagem");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private static List<OrderItem> GenerateRandomItems(Random random)
    {
        var products = new[] { "Notebook", "Mouse", "Keyboard", "Monitor", "Headset" };
        var itemCount = random.Next(1, 4);
        
        return Enumerable.Range(0, itemCount)
            .Select(i => new OrderItem
            {
                ProductId = $"PROD{random.Next(100, 999)}",
                ProductName = products[random.Next(products.Length)],
                Quantity = random.Next(1, 5),
                Price = random.Next(50, 500)
            })
            .ToList();
    }

    public override void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        base.Dispose();
    }
}