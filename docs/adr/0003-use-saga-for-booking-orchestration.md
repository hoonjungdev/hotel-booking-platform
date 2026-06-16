# Use Saga for booking orchestration

We use a Booking Saga to orchestrate the cross-module reservation workflow through integration events, including inventory hold, payment simulation, confirmation, failure, expiration, and notification requests. The Saga decides the next workflow step but does not directly change inventory, payment, or reservation aggregates, keeping domain rules inside their owning modules instead of turning the Saga into a central business service.
