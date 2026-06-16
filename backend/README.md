# Hotel Booking Backend

This solution contains the modular monolith backend for the Hotel Booking Platform.

## Projects

- `HotelBooking.Api`: ASP.NET Core Web API entry point
- `HotelBooking.AppHost`: .NET Aspire app host for local orchestration
- `HotelBooking.ServiceDefaults`: Aspire service defaults, health checks, and OpenTelemetry setup
- `HotelBooking.Worker`: background worker host for future outbox, inbox, and Saga processing
- `HotelBooking.SharedKernel`: shared domain primitives
- `HotelBooking.BuildingBlocks`: application and infrastructure building blocks
- `HotelBooking.Modules.*`: bounded module projects
- `tests/*`: unit, integration, and architecture test projects

## Commands

```bash
dotnet restore backend/HotelBooking.slnx
dotnet build backend/HotelBooking.slnx
dotnet test backend/HotelBooking.slnx
dotnet run --project backend/src/HotelBooking.AppHost
```

`docker-compose.yml` at the repository root provides standalone PostgreSQL and RabbitMQ services for development outside Aspire.
