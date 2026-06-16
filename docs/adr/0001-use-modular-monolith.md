# Use a modular monolith

We use a modular monolith for the hotel booking platform: one repository, one backend solution, one PostgreSQL database, module-specific schemas and DbContexts, and RabbitMQ for internal asynchronous workflows. This demonstrates strong domain boundaries, CQRS, Saga coordination, and reliable messaging while keeping the portfolio project runnable, reviewable, and easier to discuss than a distributed microservice system.
