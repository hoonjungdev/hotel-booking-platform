# Hotel Booking Platform

A multi-property hotel booking platform where guests reserve stays and hotel admins manage the hotels they are responsible for. This context deliberately focuses on hotel booking operations, not OTA marketplace concerns.

## Language

**Hotel Booking Platform**:
A system for searching hotels, reserving stays, and managing hotel booking operations across multiple properties.
_Avoid_: OTA marketplace, travel marketplace

**Hotel**:
A bookable property that offers room types, inventory, rates, and booking policies.
_Avoid_: Property when referring to the guest-facing accommodation

**Property Module**:
The module responsible for hotel setup and management. The module may be named Property in code, but the domain term exposed to guests and business language is Hotel.
_Avoid_: Property as the guest-facing term

**Guest**:
A person who searches for availability and makes reservations.
_Avoid_: Customer, client, user

**Guest Hotel Search**:
A simple discovery flow where a guest searches hotels by destination, stay date range, and occupancy. It returns hotels with available room types but does not include OTA marketplace ranking, ads, reviews, or channel marketplace concerns.
_Avoid_: OTA search, marketplace discovery

**Hotel Admin**:
A person who manages one or more assigned hotels.
_Avoid_: Host, supplier, vendor

**System Admin**:
A person who can manage platform-wide hotel operations across all hotels.
_Avoid_: Super user

**Hotel Assignment**:
The relationship that grants a hotel admin permission to manage a specific hotel.
_Avoid_: Vendor ownership, supplier account

**Room Type**:
A category of rooms that guests can reserve, such as Deluxe Double or Standard Twin. Reservations are made for room types, not specific physical rooms.
_Avoid_: Room when referring to what the guest books

**Room**:
A physical room inside a hotel, such as room 301. Rooms support hotel operations but are not directly selected by guests during reservation.
_Avoid_: Unit, accommodation unit

**Stay Date Range**:
The date range of a stay, with check-in included and check-out excluded. A stay from 2026-07-01 to 2026-07-03 consumes inventory for 2026-07-01 and 2026-07-02.
_Avoid_: Date range when the check-out semantics are unclear

**Check-in Date**:
The first date on which the guest occupies inventory for a stay.
_Avoid_: Start date

**Check-out Date**:
The date the guest leaves; it is not counted as an occupied inventory date.
_Avoid_: End date

**Reservation**:
A guest's request to book a room type for a stay date range under a selected rate plan. A reservation can exist before inventory is held or payment is completed.
_Avoid_: Booking when referring to the lifecycle record

**Inventory Hold**:
A temporary claim on room type inventory for every occupied date in a stay date range. It prevents the same inventory from being promised to another reservation while payment is pending.
_Avoid_: Reservation lock, temporary booking

**Pending Reservation**:
A reservation that has been created but does not yet have a resolved inventory hold outcome.
_Avoid_: Draft booking

**Awaiting Payment Reservation**:
A reservation whose inventory hold has succeeded and is waiting for payment.
_Avoid_: Pending payment when it hides whether inventory is already held

**Confirmed Reservation**:
A reservation whose payment has been authorized and whose held inventory has been converted into booked inventory.
_Avoid_: Completed booking

**Cancelled Reservation**:
A reservation that was explicitly cancelled by a guest, hotel admin, or permitted actor.
_Avoid_: Failed reservation, expired reservation

**Expired Reservation**:
A reservation that was not paid within its allowed payment window.
_Avoid_: Cancelled reservation

**Failed Reservation**:
A reservation that cannot continue because a required step such as inventory hold or payment failed.
_Avoid_: Cancelled reservation, expired reservation

**Payment**:
A simulated financial process associated with a reservation. The platform models payment outcomes without integrating a real payment gateway in the first version.
_Avoid_: Transaction when referring to the reservation-level payment process

**Payment Authorization**:
A successful simulated payment outcome that allows a reservation to be confirmed.
_Avoid_: Payment capture when capture is not part of the booking flow

**Refund**:
A simulated reversal related to a payment after cancellation or another permitted scenario.
_Avoid_: Chargeback

**Currency**:
The denomination used for a hotel price or payment amount, identified by a canonical three-letter currency code.
_Avoid_: Currency symbol when identifying the denomination

**Hotel Selling Currency**:
The single currency in which a hotel publishes rates, agrees reservation prices, and accepts payments.
_Avoid_: Default currency when the hotel does not support multiple selling currencies

**Money**:
A non-negative amount denominated in exactly one currency. Amounts in different currencies are not interchangeable or combinable.
_Avoid_: Bare decimal amount, price without currency

**Cancellation Policy**:
A penalty schedule attached to a rate plan that defines the charge for cancelling a confirmed reservation before check-in. A policy may apply different penalties to different advance-notice periods.
_Avoid_: Refund policy

**Cancellation Penalty**:
The charge selected from a cancellation policy according to when the reservation is cancelled, such as no charge, a percentage of the agreed total price, or a number of occupied nights.
_Avoid_: Cancellation fee when referring to the rule rather than its calculated monetary amount

**Flexible Rate Plan**:
A rate plan whose cancellation policy includes at least one penalty-free period before check-in. Its exact free-cancellation deadline is defined by the attached cancellation policy.
_Avoid_: Refundable plan

**Non-refundable Rate Plan**:
A rate plan whose cancellation policy charges the full agreed stay price when a confirmed reservation is cancelled before check-in.
_Avoid_: Final sale plan

**Rate Plan**:
A sellable pricing option for a room type, such as Flexible Breakfast Included or Non-refundable Room Only.
_Avoid_: Price plan, package

**Rate Plan Code**:
A hotel-defined operational identifier for a rate plan. Codes are treated without regard to letter case and presented in canonical uppercase.
_Avoid_: Price plan code, package code

**Daily Rate**:
The price for a room type under a rate plan on a specific occupied date.
_Avoid_: Static room price

**Requested Occupancy**:
The actual adult and child guest composition requested for one room in a price quote or reservation.
_Avoid_: Occupancy when referring to configured room type capacity

**Price Quote**:
A time-limited pre-reservation price offer for one room, a room type, stay date range, requested occupancy, and rate plan. It preserves the quoted nightly prices and cancellation terms but does not reserve inventory or guarantee that the hotel, room type, or rate plan remains sellable.
_Avoid_: Estimate when the system intends to honor it for a short window

**Reservation Price Snapshot**:
The final agreed price stored on a reservation at creation time. It does not change when daily rates are later updated.
_Avoid_: Current price

**Inventory Date**:
The inventory record for one hotel, one room type, and one occupied date.
_Avoid_: Inventory when the date-specific meaning matters

**Total Quantity**:
The total number of rooms available to manage for a room type on an inventory date.
_Avoid_: Capacity when referring to sellable room count

**Held Quantity**:
The number of rooms temporarily claimed by reservations that are waiting for payment.
_Avoid_: Reserved quantity

**Booked Quantity**:
The number of rooms committed to confirmed reservations.
_Avoid_: Sold quantity

**Closed Quantity**:
The number of rooms intentionally removed from sale by hotel operations.
_Avoid_: Blocked quantity

**Available Quantity**:
The remaining quantity that can be held for new reservations on an inventory date, calculated from total, held, booked, and closed quantities.
_Avoid_: Vacancy

**Atomic Inventory Hold**:
An inventory hold that succeeds for every occupied date in the stay date range or fails for all of them. The platform does not allow partial inventory holds.
_Avoid_: Partial hold

**Overbooking**:
A state where reservations claim more room type inventory than is available for one or more inventory dates.
_Avoid_: Overselling

**Hold Expiration Window**:
The time allowed for payment after inventory has been held. In the first version, the platform uses a 10-minute hold expiration window.
_Avoid_: Payment timeout when referring to the inventory hold policy
