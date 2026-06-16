# Architecture

The Hotel Booking Platform uses a modular monolith to demonstrate production-style backend architecture without the operational noise of distributed microservices.

## Goals

- Keep the project runnable and reviewable as a portfolio system.
- Show strong module boundaries.
- Demonstrate CQRS, DDD, Saga orchestration, reliable messaging, and observability.
- Keep the core domain understandable enough to discuss in interviews.

## Shape

```text
Next.js Frontend
  -> ASP.NET Core API
  -> Application Layer
       -> Commands via MediatR
       -> Queries via MediatR
       -> Validation, logging, performance, transaction, idempotency behaviors
  -> Domain Modules
       -> Property
       -> Inventory
       -> Pricing
       -> Booking
       -> Payment
       -> Notification
       -> Identity
  -> PostgreSQL

RabbitMQ
  <-> MassTransit Consumers
  <-> Booking Saga
```

## CQRS

Commands change state. They use domain models, EF Core, business rules, domain events, and outbox writes.

Queries read state. They return DTOs optimized for API and screen needs, using Dapper or EF Core `AsNoTracking` where appropriate. Query handlers must not expose domain entities.

The first version keeps one PostgreSQL database. It separates command and query responsibility without introducing separate read and write databases.

## Module Boundaries

Each module owns its domain rules and persistence concerns. Cross-module communication should happen through contracts and integration events, not by reaching into another module's infrastructure.

The initial module set:

- Property
- Inventory
- Pricing
- Booking
- Payment
- Notification
- Identity

The code may use `Property` as a module name, but the domain language exposed to guests and business discussions is `Hotel`.

## Reliable Messaging

Integration events that cross module boundaries use Outbox and Inbox patterns.

Outbox is required when a database state change must publish an integration event. Inbox is required for consumers handling RabbitMQ integration events.

Internal domain events that remain inside a transaction do not need to be published through RabbitMQ.

## Architecture Decisions

See:

- [ADR 0001: Use a modular monolith](./adr/0001-use-modular-monolith.md)
- [ADR 0002: Use CQRS with a shared database](./adr/0002-use-cqrs-with-shared-database.md)
- [ADR 0003: Use Saga for booking orchestration](./adr/0003-use-saga-for-booking-orchestration.md)
- [ADR 0004: Use Outbox and Inbox for integration events](./adr/0004-use-outbox-inbox-for-integration-events.md)

