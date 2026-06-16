# Testing Strategy

Testing is a primary portfolio signal for this project. The test suite should prove business rules, persistence behavior, messaging reliability, architecture boundaries, and inventory consistency under concurrency.

## Unit Tests

Initial required tests:

- Stay date range counts nights with check-in included and check-out excluded.
- Inventory date calculates available quantity correctly.
- Inventory hold fails when available quantity is insufficient.
- Reservation starts as `Pending`.
- Confirmed reservation cannot expire.
- Cancelled reservation cannot be confirmed.
- Total price is calculated by summing daily rates.
- Cancellation policy determines whether a refund is allowed.

## Integration Tests

Initial required tests:

- Creating a reservation saves the reservation.
- Creating a reservation creates an outbox message.
- Inventory hold succeeds for all occupied dates when inventory is available.
- Inventory hold fails atomically if any occupied date is unavailable.
- Inbox prevents duplicate message processing.

Use Testcontainers for PostgreSQL and RabbitMQ where possible. Use Respawn or an equivalent reset strategy to keep integration tests isolated.

## Architecture Tests

Initial rules:

- Domain projects must not reference Infrastructure.
- Domain projects must not reference API or Presentation.
- Application projects must not reference API or Presentation.
- Modules must not reference other modules' Infrastructure projects.
- Cross-module communication should happen through Contracts or integration events.

## Concurrency Tests

The main concurrency test should be added as soon as inventory hold persistence is implemented:

```text
Given available quantity is 1,
When 20 concurrent reservation attempts target the same hotel, room type, and stay date range,
Then only one hold succeeds,
And the remaining attempts fail without overbooking.
```

This test is mandatory before the project is presented as a serious backend portfolio.

