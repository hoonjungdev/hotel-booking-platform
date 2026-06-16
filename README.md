# Hotel Booking Platform

A production-style hotel booking platform built to demonstrate backend engineering ability through hotel reservation domain modeling, CQRS, DDD, event-driven workflows, reliable messaging, inventory consistency, and automated testing.

The project is primarily an overseas backend engineering portfolio. It is also shaped so it can become an Upwork-ready booking platform demo, but the first priority is proving backend correctness and architectural judgment.

## Product Scope

The platform supports multiple hotels. Guests can search hotels by destination, stay date range, and occupancy, then reserve a room type under a selected rate plan. Hotel admins manage the hotels assigned to them, including room types, inventory, rates, reservations, and operational event timelines.

This is not an OTA marketplace. The first version intentionally excludes marketplace ranking, ads, reviews, supplier settlement, channel manager integration, and commission management.

## Core Capabilities

- Guest hotel search by destination, dates, and occupancy
- Room type based reservation, not physical room selection
- Rate plan and daily rate based pricing
- Reservation price snapshot at creation time
- Inventory hold before payment
- Payment authorization simulation
- Reservation confirmation, cancellation, expiration, and failure
- Atomic multi-night inventory hold
- Overbooking prevention under concurrent requests
- Booking Saga orchestration
- Outbox and Inbox patterns for reliable integration events
- Admin inventory calendar, rate calendar, reservation view, event timeline, and outbox monitor
- Unit, integration, architecture, and concurrency tests

## Architecture

The system uses a modular monolith: one repository, one backend solution, one PostgreSQL database, module-specific schemas and DbContexts, and RabbitMQ for asynchronous cross-module workflows.

Primary backend stack:

- .NET 10
- ASP.NET Core Web API
- EF Core
- PostgreSQL
- MediatR
- FluentValidation
- MassTransit
- RabbitMQ
- Dapper
- Serilog
- OpenTelemetry
- .NET Aspire
- Docker / Docker Compose

Frontend stack:

- Next.js
- TypeScript
- Tailwind CSS
- shadcn/ui
- TanStack Query
- React Hook Form
- Zod

## Modules

- Property: hotel setup, room types, rooms, amenities, policies
- Inventory: inventory dates, inventory holds, availability consistency
- Pricing: rate plans, daily rates, price quotes, cancellation policies
- Booking: reservation lifecycle and event timeline
- Payment: simulated payment authorization, failure, and refund
- Notification: notification logs and booking-related notification simulation
- Identity: guests, hotel admins, system admins, and hotel assignments

## First Vertical Slice

The first implementation slice is `Reservation Happy Path with Inventory Hold`:

1. Seed hotel, room type, inventory, rate plan, and daily rates.
2. Guest searches availability.
3. Guest requests a price quote.
4. Guest creates a reservation.
5. Reservation starts as `Pending`.
6. Outbox publishes `reservation.created.v1`.
7. Booking Saga requests an inventory hold.
8. Inventory hold succeeds atomically for all occupied dates.
9. Reservation becomes `AwaitingPayment`.

Payment success, payment failure, confirmation, release, and expiration follow in the next slice.

## Documentation

- [Context glossary](./CONTEXT.md)
- [Architecture](./docs/architecture.md)
- [Booking flow](./docs/booking-flow.md)
- [Inventory consistency](./docs/inventory-consistency.md)
- [Testing strategy](./docs/testing-strategy.md)
- [Architecture decisions](./docs/adr/)

## Local Backend Commands

```bash
dotnet restore backend/HotelBooking.slnx
dotnet build backend/HotelBooking.slnx
dotnet test backend/HotelBooking.slnx
dotnet run --project backend/src/HotelBooking.AppHost
```

Standalone infrastructure is available with:

```bash
docker compose up -d
```
