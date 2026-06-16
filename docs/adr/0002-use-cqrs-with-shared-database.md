# Use CQRS with a shared database

We use full CQRS by separating command handlers from query handlers: commands use domain models, EF Core, business rules, domain events, and outbox writes, while queries return read DTOs optimized for API and screen needs. We keep a shared PostgreSQL database in the first version instead of separate read and write databases so the project demonstrates command/query responsibility separation without adding unnecessary distributed data complexity.
