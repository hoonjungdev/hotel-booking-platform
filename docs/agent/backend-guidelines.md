# Backend Guidelines

Apply these rules to new and modified code. Improve existing violations when their code is touched, but do not expand focused work into unrelated cleanup unless requested.

## Boundaries

- Follow the modular-monolith, DDD, and CQRS boundaries defined in [`docs/architecture.md`](../architecture.md).
- Validate request shape and format at the application boundary. Enforce business invariants inside domain models even when the application layer already validates the request.
- Keep domain rules inside domain models, not controllers or infrastructure.
- Keep module persistence private. Cross-module workflows use contracts and integration events.
- Commands change state through domain models. Queries return screen/API DTOs and never expose domain or EF Core entities.

## Time and Asynchronous Work

- Do not read the system clock directly inside domain models.
- Obtain the current time through `TimeProvider` at the application boundary and pass explicit timestamps to domain behavior.
- Asynchronous application and infrastructure operations must accept and propagate `CancellationToken`.
- Do not block asynchronous work with `.Result`, `.Wait()`, or equivalent sync-over-async patterns.

## Reliability

- Persist command-side state changes and their Outbox messages in the same database transaction.
- Never publish an integration event before the related transaction commits.
- Treat message delivery as at-least-once and make consumers idempotent through Inbox processing.
- Preserve atomic multi-night inventory holds and prove overbooking prevention as defined in [`docs/inventory-consistency.md`](../inventory-consistency.md).

## C# XML Documentation

Use XML documentation to make domain intent and public contracts explicit.

Documentation is required for:

- Public and protected classes, records, structs, interfaces, enums, delegates, constructors, methods, properties, and events
- Every public enum member
- Aggregates, entities, value objects, strongly typed IDs, domain events, commands, queries, and integration events
- Methods that enforce business rules, perform state transitions, or cause side effects, regardless of accessibility

It may be omitted for obvious private helpers, self-explanatory private members, generated code, and clearly named test methods. Interface implementations and overrides may use `<inheritdoc />`.

Every documentation comment must contain a meaningful `<summary>` unless it uses `<inheritdoc />`. Describe intent, domain meaning, invariants, side effects, or failure conditions instead of repeating the signature. Add `<param>`, `<typeparam>`, `<returns>`, and `<exception>` when they clarify the contract. Keep comments concise, aligned with [`CONTEXT.md`](../../CONTEXT.md), and current with behavior.

Apply the documentation requirement to every newly added or modified symbol in scope.

## Verification

Follow [`docs/testing-strategy.md`](../testing-strategy.md) for the evidence required by a backend change.
