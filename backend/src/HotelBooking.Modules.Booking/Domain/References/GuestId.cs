using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.References;

/// <summary>References the authenticated Guest that creates a Reservation without depending on Identity.</summary>
public readonly record struct GuestId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private GuestId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("GuestId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Reconstructs a Guest reference from an identifier owned by Identity.</summary>
    public static GuestId From(Guid value)
    {
        return new GuestId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
