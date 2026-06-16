# Booking Flow

The booking flow is the main product and architecture demonstration. It shows how a guest reservation moves through inventory hold, payment simulation, confirmation, failure, expiration, and notification.

## Guest Flow

1. Guest searches hotels by destination, stay date range, and occupancy.
2. Guest selects a hotel, room type, and rate plan.
3. Guest requests a price quote.
4. Guest creates a reservation.
5. The system holds inventory before payment.
6. Guest completes simulated payment.
7. The reservation is confirmed or failed based on the payment outcome.

## Date Semantics

Stay date ranges include check-in and exclude check-out.

Example:

```text
Check-in: 2026-07-01
Check-out: 2026-07-03
Occupied dates:
- 2026-07-01
- 2026-07-02
```

## Reservation Statuses

- `Pending`: reservation created, inventory hold outcome not resolved yet
- `AwaitingPayment`: inventory hold succeeded, payment is pending
- `Confirmed`: payment authorization succeeded and held inventory became booked inventory
- `Cancelled`: explicitly cancelled by a permitted actor
- `Expired`: payment was not completed within the hold expiration window
- `Failed`: inventory hold or payment failed

## Saga Responsibility

The Booking Saga orchestrates cross-module workflow. It decides the next workflow step but does not directly change reservation, inventory, or payment aggregates.

Happy path:

```text
ReservationCreated
  -> InventoryHoldRequested
  -> InventoryHeld
  -> PaymentRequested
  -> PaymentAuthorized
  -> ReservationConfirmed
  -> NotificationRequested
```

Inventory failure path:

```text
ReservationCreated
  -> InventoryHoldRequested
  -> InventoryHoldFailed
  -> ReservationFailed
```

Payment failure path:

```text
ReservationCreated
  -> InventoryHeld
  -> PaymentRequested
  -> PaymentFailed
  -> InventoryHoldReleased
  -> ReservationFailed
```

Expiration path:

```text
ReservationCreated
  -> InventoryHeld
  -> AwaitingPayment
  -> ReservationExpired
  -> InventoryHoldReleased
```

## First Vertical Slice

The first slice stops at `AwaitingPayment`:

```text
Availability search
  -> Price quote
  -> Create reservation
  -> Pending reservation
  -> reservation.created.v1 outbox message
  -> Saga starts
  -> Inventory hold requested
  -> Inventory held
  -> AwaitingPayment reservation
```

Payment authorization, failure, confirmation, release, and expiration are implemented in the next slice.

