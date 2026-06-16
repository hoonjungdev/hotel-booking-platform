# Use Outbox and Inbox for integration events

We require the Outbox pattern when a database state change must publish an integration event, and we require the Inbox pattern for every RabbitMQ consumer that handles integration events. Internal domain events that stay within the same transaction do not need to be published through RabbitMQ, which keeps reliable messaging focused on cross-module consistency without turning every domain change into distributed messaging.
