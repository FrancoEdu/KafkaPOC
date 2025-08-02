using Confluent.Kafka;
using KafkaPOC.Core.Configuration;
using KafkaPOC.Core.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KafkaPOC.Consumer;

public class KafkaConsumerService : BackgroundService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly KafkaSettings _kafkaSettings;
    private readonly IConsumer<string, string> _consumer;

    public KafkaConsumerService(
        ILogger<KafkaConsumerService> logger,
        IOptions<KafkaSettings> kafkaSettings)
    {
        _logger = logger;
        _kafkaSettings = kafkaSettings.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = _kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // Commit manual para controle
            SessionTimeoutMs = _kafkaSettings.SessionTimeoutMs
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Erro no Consumer: {Error}", e.Reason))
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka Consumer Service iniciado");

        _consumer.Subscribe(Topics.Orders);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                
                if (consumeResult?.Message != null)
                {
                    await ProcessMessage(consumeResult);
                    
                    // Commit manual após processamento bem-sucedido
                    _consumer.Commit(consumeResult);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Erro ao consumir mensagem: {Error}", ex.Error.Reason);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no consumer");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ProcessMessage(ConsumeResult<string, string> consumeResult)
    {
        try
        {
            var orderEvent = JsonConvert.DeserializeObject<OrderEvent>(consumeResult.Message.Value);
            
            _logger.LogInformation(
                "Processando pedido - OrderId: {OrderId}, Customer: {CustomerId}, Amount: {Amount}, Items: {ItemCount}",
                orderEvent.OrderId, orderEvent.CustomerId, orderEvent.Amount, orderEvent.Items.Count);

            // Simular processamento
            await SimulateOrderProcessing(orderEvent);

            _logger.LogInformation("Pedido processado com sucesso - OrderId: {OrderId}", orderEvent.OrderId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar mensagem do tópico {Topic}, Partition: {Partition}, Offset: {Offset}",
                consumeResult.Topic, consumeResult.Partition.Value, consumeResult.Offset.Value);
        }
    }

    private async Task SimulateOrderProcessing(OrderEvent orderEvent)
    {
        // Simular validação
        await Task.Delay(100);
        
        // Simular processamento de items
        foreach (var item in orderEvent.Items)
        {
            _logger.LogDebug("Processando item: {ProductName} x{Quantity}", 
                item.ProductName, item.Quantity);
            await Task.Delay(50);
        }
        
        // Simular atualização no banco
        await Task.Delay(200);
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}