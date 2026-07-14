using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.References;

/// <summary>References the reservation that owns a hold without depending on the Booking module.</summary>
public readonly record struct ReservationId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private ReservationId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("ReservationId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new reservation reference identifier.</summary>
    public static ReservationId Create()
    {
        return new ReservationId(Guid.NewGuid());
    }

    /// <summary>Creates a reservation reference from an existing identifier value.</summary>
    public static ReservationId From(Guid value)
    {
        return new ReservationId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
