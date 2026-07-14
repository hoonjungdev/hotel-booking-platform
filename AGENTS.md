# AGENTS.md

## Required Workflow

1. Always respond to the user in Korean.
2. Before starting work, read [`docs/agent/working-agreement.md`](docs/agent/working-agreement.md).
3. Read the applicable reference documents below before inspecting or changing relevant files.
4. Treat `CONTEXT.md` as the canonical source for domain language. If the user's terminology conflicts with it, clarify the intended meaning before proceeding.

## Task References

| Task | Required documents |
| --- | --- |
| Product scope or general orientation | [`README.md`](README.md) |
| Domain modeling or terminology | [`CONTEXT.md`](CONTEXT.md) |
| Architecture or module boundaries | [`docs/architecture.md`](docs/architecture.md), relevant [`docs/adr/`](docs/adr/) |
| Reservation workflow or state transitions | [`docs/booking-flow.md`](docs/booking-flow.md) |
| Inventory or overbooking | [`docs/inventory-consistency.md`](docs/inventory-consistency.md) |
| Backend production code | [`docs/agent/backend-guidelines.md`](docs/agent/backend-guidelines.md), [`docs/testing-strategy.md`](docs/testing-strategy.md) |
| Automated tests or testing strategy | [`docs/testing-strategy.md`](docs/testing-strategy.md) |
| Frontend implementation | [`docs/agent/frontend-guidelines.md`](docs/agent/frontend-guidelines.md) |
| Meaningful implementation, concept explanation, or guided learning | [`docs/agent/teaching-guidelines.md`](docs/agent/teaching-guidelines.md) |

Read every row that applies before making or recommending a change. More specific `AGENTS.md` files, when present in a subdirectory, may add or override instructions for that scope.
