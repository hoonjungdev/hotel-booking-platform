# Testing Strategy

Testing is a primary portfolio signal for this project. The suite must prove business rules, persistence behavior, API contracts, messaging reliability, architecture boundaries, and inventory consistency under concurrency.

## Principles

- Test observable behavior and domain outcomes, not private implementation details.
- Use the lowest test level that provides confidence, then add integration coverage when correctness depends on PostgreSQL, EF Core mappings, transactions, serialization, RabbitMQ, or concurrency.
- Keep tests deterministic with explicit timestamps, stable identifiers, and controlled external dependencies.
- Name tests by behavior and condition so a failure explains the broken rule.
- Add a regression test for every fixed behavioral defect.
- Treat coverage as a diagnostic signal, not a substitute for meaningful assertions.

A required scenario becomes mandatory when its corresponding production behavior is introduced. Testing requirements do not authorize implementing out-of-scope production behavior.

Apply this strategy to new and modified tests and production behavior. Improve existing violations when their code or tests are touched, but do not expand focused work into unrelated cleanup unless requested.

## Tests as Executable Documentation

Domain unit tests and application use-case tests must explain behavior using the canonical hotel language from [`CONTEXT.md`](../CONTEXT.md). A reader should be able to understand the protected rule or workflow without inspecting the implementation first.

- A domain unit test describes a business rule, state transition, invariant, or rejected behavior through the domain model's public API.
- An application use-case test describes an actor's request, relevant preconditions, application outcome, and observable side effects without repeating aggregate-level tests.
- Prefer scenario-oriented test names and explicit domain-relevant values in the arrangement.
- Keep one primary behavior per test so each failure identifies one broken rule.
- Use builders, fixtures, and helper methods to remove noise, not to hide the conditions that make the scenario meaningful.
- Do not couple tests to private methods or incidental implementation details.
- Prefer expressive names and Given/When/Then or Arrange/Act/Assert structure over explanatory comments. Add comments only when the scenario cannot otherwise be made clear.

## Required Evidence by Change

| Change | Required evidence |
| --- | --- |
| Value object, entity, aggregate, pricing rule, or state transition | Unit tests for valid behavior, boundaries, and rejected transitions |
| Command/query handler, validation, or application orchestration | Focused use-case tests; add integration coverage when the outcome depends on a real boundary |
| EF Core mapping, query, migration, constraint, or transaction | PostgreSQL integration tests |
| HTTP endpoint or API contract | API integration tests through the HTTP boundary |
| Outbox persistence | PostgreSQL integration tests proving state and message atomicity, including rollback |
| Outbox dispatch or publication | Integration tests proving only committed messages are dispatched and failed attempts remain recoverable |
| Inbox processing | Integration tests proving duplicate delivery does not repeat state changes or side effects |
| Consumer behavior | Integration tests for successful handling and each configured retry or failure path |
| Saga orchestration or persistence | Integration tests for the happy path and each implemented failure, compensation, timeout, and recovery path |
| Versioned integration event contract | Contract tests for event identity, required fields, serialization, and compatibility |
| Module dependency rule | Architecture tests |
| Inventory mutation under competing requests | PostgreSQL concurrency tests |

## Unit Tests

Unit tests should exercise domain behavior through public APIs without mocking domain models. Cover success, boundary values, invariant violations, and invalid state transitions.

Initial required scenarios:

- Stay date range counts nights with check-in included and check-out excluded.
- Inventory date calculates available quantity correctly.
- Inventory date rejects increasing held quantity when available quantity is insufficient.
- Inventory hold is valid before expiration and expired at the expiration boundary.
- Reservation starts as `Pending`.
- Confirmed reservation cannot expire.
- Cancelled reservation cannot be confirmed.
- Total price is calculated by summing daily rates.
- Cancellation policy selects the applicable penalty at each advance-notice boundary.

## Application Use-Case Tests

Use-case tests explain how the application coordinates domain behavior for an actor's request. Keep them fast and independent of databases, message brokers, and HTTP unless that boundary is the behavior under test.

- Exercise a command handler, query handler, or application service through its public input.
- Replace repositories, clocks, message publishers, and external services only at explicit application ports. Prefer simple fakes when they make state and outcomes clearer than interaction-heavy mocks.
- Use real aggregates and value objects; do not mock the domain model.
- Assert the application result and meaningful observable effects, such as persisted state requested through a port, raised events, or an outgoing message request.
- Verify call counts or ordering only when they are part of the use-case contract, such as idempotency or required sequencing.
- Do not repeat aggregate-level invariant tests or claim database, transaction, serialization, or transport correctness.

Place these tests under `HotelBooking.UnitTests/Application/<Module>/<UseCase>` while the suite is small. Introduce a separate application test project only when its size or dependencies justify it.

## Integration and API Tests

Use Testcontainers with PostgreSQL for behavior that depends on persistence, constraints, transactions, isolation, or concurrency. Do not use EF Core InMemory or SQLite to claim PostgreSQL behavior.

Use RabbitMQ Testcontainers when testing transport configuration, delivery, retry, or consumer integration. Keep narrower consumer tests below the transport boundary when RabbitMQ itself is not part of the behavior under test.

Apply production migrations when constructing the test database. Reset state with Respawn or an equivalent strategy, and give parallel tests isolated data. Tests must not depend on execution order or state left by another test.

Initial required scenarios:

- Creating a reservation persists the reservation and its price snapshot.
- Creating a reservation persists its Outbox message in the same transaction.
- A failed reservation transaction persists neither the reservation nor its Outbox message.
- Inventory hold succeeds for every occupied date when inventory is available.
- Inventory hold fails atomically if any occupied date is unavailable.
- Inbox prevents duplicate message processing and duplicate side effects.
- Completed HTTP endpoints return the documented status, validation error shape, and response contract.

## Messaging and Saga Tests

Reliable messaging tests must assume at-least-once delivery.

- Prove an Outbox message is not observable before its state transaction commits.
- Prove retries do not create duplicate externally visible effects.
- Deliver the same integration message more than once and prove Inbox idempotency.
- Add contract tests for each versioned integration event. Verify its name and version, serialization of required fields, and compatibility expectations. Breaking contract changes require a new event version.
- Cover the Saga happy path and each implemented failure, compensation, and timeout path from [`docs/booking-flow.md`](booking-flow.md).
- Prove Saga state can resume correctly after redelivery or process restart when persistence is implemented.

## Architecture Tests

Required rules:

- A module project must not reference another module's implementation assembly. Dedicated Contracts assemblies are allowed when introduced.
- Types in a module's Domain namespace must not depend on API, Worker, EF Core, MassTransit, or another module's implementation types.
- Cross-module communication must use Contracts or integration events rather than another module's persistence or internal types.
- Query and API models must not expose domain or EF Core entities.

These rules match the current module-per-project structure. If Domain, Application, and Infrastructure become separate projects later, add project-reference rules that enforce the same dependency direction.

## Concurrency Tests

The first reservation slice requires this hold-level scenario against PostgreSQL:

```text
Given available quantity is 1,
When 20 independent reservations concurrently request a hold
For the same hotel, room type, and stay date range,
Then exactly one hold succeeds,
And every other attempt fails,
And every inventory date preserves its quantity invariants.
```

When payment authorization and confirmation are implemented, also prove the canonical end-to-end portfolio scenario:

```text
Given available quantity is 1 for the same hotel, room type, and stay date range,
When 20 independent reservation workflows compete concurrently,
Then exactly one reservation reaches Confirmed,
And no inventory date is overbooked.
```

The hold-level test proves the consistency mechanism used by the first slice. The end-to-end test proves the completed booking claim from [`docs/inventory-consistency.md`](inventory-consistency.md). Start competing operations from the same synchronization point and assert persisted final state, not only returned results. Do not use an in-memory store to prove overbooking prevention.

## Test Reliability

- Use fixed timestamps whenever time is part of the arrangement or expected outcome. Use a controlled `TimeProvider` for application behavior and do not make correctness depend on the wall clock.
- Do not use arbitrary `Thread.Sleep` or `Task.Delay` calls to wait for asynchronous outcomes. Use bounded polling tied to an observable condition.
- Keep test data explicit. Use builders or fixtures only when they improve readability without hiding domain-relevant values.
- A flaky test is a defect. Fix its synchronization, isolation, or dependency control instead of adding retries that hide the cause.

## Vertical Slice Completion

Each completed vertical slice needs focused tests at the appropriate levels and acceptance-style evidence proving its main workflow through its primary entry point and the real boundaries required for that claim. The evidence may be split across multiple tests, but together they must prove the slice's declared final outcome and correlate the same workflow using stable business or message identifiers. An acceptance test does not need to include every infrastructure dependency.

For the first reservation slice, use one acceptance test for the HTTP request through Reservation and Outbox persistence in PostgreSQL, and a separate asynchronous acceptance test through RabbitMQ, the Booking Saga, Inventory, and the `AwaitingPayment` Reservation state when that workflow is implemented. At each handoff, assert that the same `ReservationId` or message correlation identifier is preserved so the combined evidence proves the complete slice. Keep the tests independent; they must not share state or depend on execution order.

Add critical failure-path acceptance tests where the portfolio claim depends on them. Acceptance tests complement rather than replace unit, integration, architecture, and concurrency tests.

A behavioral change is not complete until its relevant tests pass. Run the narrowest affected tests during development, then the broader relevant suite before delivery, and report any skipped checks or remaining failures.
