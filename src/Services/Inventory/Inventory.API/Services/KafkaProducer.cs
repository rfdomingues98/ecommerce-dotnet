using System.Text.Json;
using Confluent.Kafka;
using Inventory.API.Events;

namespace Inventory.API.Services;

public interface IEventProducer
{
    Task ProduceAsync<T>(string topic, T eventData) where T : InventoryEvent;
}

public class KafkaProducer : IEventProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            Acks = Acks.All,
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
        _logger = logger;
    }

    public async Task ProduceAsync<T>(string topic, T eventData) where T : InventoryEvent
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = eventData.Id.ToString(),
                Value = JsonSerializer.Serialize(eventData)
            };

            DeliveryResult<string, string> deliveryResult = await _producer.ProduceAsync(topic, message);
            _logger.LogInformation($"Event delivered to {deliveryResult.Topic} at partition {deliveryResult.Partition} with offset {deliveryResult.Offset}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error producing event to Kafka: {ex.Message}");
            throw;
        }
    }

    public void Dispose() => _producer.Dispose();
}