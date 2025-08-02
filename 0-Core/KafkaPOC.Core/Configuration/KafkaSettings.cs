namespace KafkaPOC.Core.Configuration;

public class KafkaSettings
{
	public string BootstrapServers { get; set; } = "localhost:9092";
	public string GroupId { get; set; } = "poc-consumer-group";
	public string AutoOffsetReset { get; set; } = "earliest";
	public int SessionTimeoutMs { get; set; } = 6000;
	public bool EnableAutoCommit { get; set; } = true;
	public string SecurityProtocol { get; set; } = "plaintext";
}