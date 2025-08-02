# 🚀 Kafka POC - Event-Driven Architecture com .NET

<div align="center">

![Kafka](https://img.shields.io/badge/Apache%20Kafka-231F20?style=for-the-badge&logo=apache-kafka&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)

*Uma implementação completa de Event-Driven Architecture usando Apache Kafka e .NET Worker Services*

[🎯 Funcionalidades](#-funcionalidades) •
[🏗️ Arquitetura](#️-arquitetura) •
[🚀 Quick Start](#-quick-start) •
[📖 Documentação](#-documentação) •
[🔧 Configuração](#-configuração)

</div>

---

## 📋 Sobre o Projeto

Este projeto demonstra como implementar uma arquitetura orientada a eventos robusta usando **Apache Kafka** e **.NET Worker Services**. Simula um sistema de processamento de pedidos em tempo real, mostrando as melhores práticas para produção e consumo de mensagens distribuídas.

### 🎯 Funcionalidades

- ✅ **Producer Assíncrono**: Geração contínua de eventos de pedidos
- ✅ **Consumer Resiliente**: Processamento confiável com commit manual
- ✅ **Tratamento de Erros**: Retry automático e dead letter queue
- ✅ **Observabilidade**: Logging estruturado e métricas
- ✅ **Graceful Shutdown**: Parada adequada dos serviços
- ✅ **Docker Support**: Ambiente completo containerizado
- ✅ **Kafka UI**: Interface web para monitoramento
- ✅ **Configuração Flexível**: Settings externalizados

---

## 🚀 Quick Start

### Pré-requisitos

- 🔵 [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- 🐳 [Docker & Docker Compose](https://docs.docker.com/get-docker/)
- 🎯 [Git](https://git-scm.com/)

### 🏃‍♂️ Executando em 3 passos

```bash
# 1️⃣ Clone o repositório
git clone https://github.com/seu-usuario/kafka-poc.git
cd kafka-poc

# 2️⃣ Suba o ambiente Kafka
docker-compose up -d

# 3️⃣ Execute os serviços
dotnet run --project KafkaPOC.Producer    # Terminal 1
dotnet run --project KafkaPOC.Consumer    # Terminal 2
```

### 🎉 Pronto!

- 📊 **Kafka UI**: http://localhost:8080
- 📝 **Logs**: Acompanhe nos terminais
- 🔍 **Monitoramento**: Veja mensagens em tempo real

---

## 📁 Estrutura do Projeto

```
KafkaPOC/
├── 🏭 KafkaPOC.Producer/           # Serviço produtor de eventos
│   ├── Services/
│   │   └── KafkaProducerService.cs # Worker service principal
│   ├── Program.cs                  # Configuração DI e Host
│   └── appsettings.json           # Configurações do producer
│
├── 🏪 KafkaPOC.Consumer/           # Serviço consumidor de eventos  
│   ├── Services/
│   │   └── KafkaConsumerService.cs # Worker service principal
│   ├── Program.cs                  # Configuração DI e Host
│   └── appsettings.json           # Configurações do consumer
│
├── 📦 KafkaPOC.Shared/             # Biblioteca compartilhada
│   ├── Models/
│   │   ├── OrderEvent.cs          # Modelo de evento de pedido
│   │   └── UserEvent.cs           # Modelo de evento de usuário
│   ├── Configuration/
│   │   └── KafkaSettings.cs       # Configurações do Kafka
│   └── Constants/
│       └── Topics.cs              # Definição dos tópicos
│
├── 🐳 docker-compose.yml           # Kafka + Zookeeper + UI
├── 📖 README.md                    # Este arquivo
└── 🔧 KafkaPOC.sln                # Solution do projeto
```

---

## ⚙️ Configuração

### 🔧 Kafka Settings

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "order-processing-group",
    "AutoOffsetReset": "earliest",
    "SessionTimeoutMs": 6000,
    "EnableAutoCommit": false,
    "BatchSize": 16384,
    "LingerMs": 10,
    "Acks": "all",
    "Retries": 3
  }
}
```

### 🎛️ Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|---------|
| `KAFKA_BOOTSTRAP_SERVERS` | Endereços dos brokers Kafka | `localhost:9092` |
| `KAFKA_GROUP_ID` | ID do grupo de consumidores | `poc-consumer-group` |
| `LOG_LEVEL` | Nível de log | `Information` |
| `PRODUCER_INTERVAL_MS` | Intervalo entre mensagens | `5000` |

### 🐳 Docker Compose Services

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| **Zookeeper** | `2181` | Coordenação do cluster Kafka |
| **Kafka** | `9092` | Broker principal |
| **Kafka UI** | `8080` | Interface web de monitoramento |

---

## 📖 Documentação Detalhada

### 🏭 Producer Service

O **KafkaProducerService** é um Worker Service que:

- 🔄 Gera eventos de pedidos automaticamente
- 🎯 Particiona mensagens por `CustomerId`
- 🔁 Implementa retry automático
- 📊 Registra métricas de produção
- ⚡ Suporte a produção assíncrona

```csharp
// Exemplo de uso
var orderEvent = new OrderEvent
{
    OrderId = Guid.NewGuid(),
    CustomerId = "CUST001",
    Amount = 299.99m,
    Status = "Created"
};

await _producer.ProduceAsync(Topics.Orders, orderEvent);
```

### 🏪 Consumer Service

O **KafkaConsumerService** é um Worker Service que:

- 📥 Consome mensagens de forma contínua
- ✅ Commit manual para garantir processamento
- 🛡️ Tratamento robusto de erros
- 🔄 Reprocessamento automático em falhas
- 📈 Monitoramento de performance

```csharp
// Processamento com tratamento de erro
try 
{
    await ProcessOrderAsync(orderEvent);
    _consumer.Commit(consumeResult);
}
catch (Exception ex)
{
    await HandleProcessingError(ex, consumeResult);
}
```

---

## 🚀 Deploy

### 🐳 Docker

```bash
# Build das imagens
docker build -t kafka-poc-producer ./KafkaPOC.Producer
docker build -t kafka-poc-consumer ./KafkaPOC.Consumer

# Executar com docker-compose
docker-compose -f docker-compose.prod.yml up -d
```

---

## 🛠️ Desenvolvimento

### 🔧 Configuração do Ambiente

```bash
# Instalar dependências
dotnet restore

# Configurar hooks pre-commit
git config core.hooksPath .githooks
chmod +x .githooks/pre-commit

# Executar formatação
dotnet format
```

### 📋 Coding Standards

- ✅ Seguir convenções C#/.NET
- ✅ Cobertura de testes > 80%
- ✅ Documentação inline
- ✅ Logging estruturado
- ✅ Tratamento de erros robusto