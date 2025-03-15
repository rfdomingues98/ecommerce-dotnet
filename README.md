# E-commerce Inventory System with Kafka, Redis and .NET

A microservices-based e-commerce inventory system using:
- .NET 8
- Apache Kafka for event messaging
- Redis for caching
- SQL Server for persistence
- Docker for containerization

## Getting Started

### Prerequisites
- Docker and Docker Compose
- .NET 8 SDK (for local development)

### Running the Application
```bash
# Start all services
docker compose up -d

# Check status
docker compose ps

# View logs
docker compose logs -f

# Stop all services
docker compose down
```

### Accessing Services
- Inventory API: http://localhost:5101
- Order API: http://localhost:5102
- Product API: http://localhost:5103
- Notification API: http://localhost:5104
- Kafka UI: http://localhost:8080
- Redis Commander: http://localhost:8082
- SQL Server: localhost:1433

## System design

See more [here](docs/system-design/SYSTEM_DESIGN.md)