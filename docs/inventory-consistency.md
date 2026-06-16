# Inventory Consistency

Inventory consistency is one of the most important technical demonstrations in this project. The platform must prevent overbooking under concurrent reservation attempts.

## Inventory Unit

Inventory is managed per hotel, room type, and occupied date.

Guests reserve room types, not physical rooms. Physical rooms can exist for hotel operations, but guest reservations claim room type inventory.

## Quantity Model

```text
Available Quantity = Total Quantity - Held Quantity - Booked Quantity - Closed Quantity
```

- `Total Quantity`: total rooms available to manage for the room type and date
- `Held Quantity`: rooms temporarily claimed by reservations awaiting payment
- `Booked Quantity`: rooms committed to confirmed reservations
- `Closed Quantity`: rooms intentionally removed from sale by hotel operations

## Atomic Multi-night Hold

A multi-night stay must hold inventory for every occupied date.

If any date cannot be held, the entire hold fails. Partial holds are not allowed.

Example:

```text
Stay: 2026-07-01 to 2026-07-04
Occupied dates:
- 2026-07-01 available
- 2026-07-02 unavailable
- 2026-07-03 available
```

Result:

```text
Inventory hold fails.
No occupied date increases Held Quantity.
```

## Hold Expiration

The first version uses a 10-minute hold expiration window.

If payment is not authorized before the window expires, the reservation becomes `Expired` and held inventory is released.

## Overbooking Test Scenario

The mandatory portfolio concurrency scenario:

```text
Given only one Deluxe Double room is available,
When 20 users try to book it concurrently,
Then only one reservation can be confirmed,
And no inventory date is overbooked.
```

The implementation should use PostgreSQL transactions and conditional updates or locking so inventory hold succeeds only when all occupied dates still have available quantity.

