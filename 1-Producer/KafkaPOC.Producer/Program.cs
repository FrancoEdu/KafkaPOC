using KafkaPOC.Core.Configuration;
using KafkaPOC.Producer;

var builder = Host.CreateApplicationBuilder(args);

// Configurações
builder.Services.Configure<KafkaSettings>(
	builder.Configuration.GetSection("Kafka"));

// Registrar serviços
builder.Services.AddHostedService<KafkaProducerService>();

// Logging
builder.Services.AddLogging(logging =>
{
	logging.AddConsole();
	logging.SetMinimumLevel(LogLevel.Information);
});

var host = builder.Build();
host.Run();