using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.References;

/// <summary>References the hotel that owns inventory without depending on the Property module.</summary>
public readonly record struct HotelId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private HotelId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("HotelId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new hotel reference identifier.</summary>
    public static HotelId Create()
    {
        return new HotelId(Guid.NewGuid());
    }

    /// <summary>Creates a hotel reference from an existing identifier value.</summary>
    public static HotelId From(Guid value)
    {
        return new HotelId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
