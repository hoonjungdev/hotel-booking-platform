using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.References;

/// <summary>References the room type sold by a rate plan without depending on the Property module.</summary>
public readonly record struct RoomTypeId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private RoomTypeId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("RoomTypeId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Reconstructs a room type reference from an identifier owned by Property.</summary>
    /// <param name="value">The existing non-empty RoomType identifier.</param>
    /// <returns>A Pricing-local reference to the RoomType.</returns>
    /// <exception cref="DomainArgumentException">Thrown when the identifier is empty.</exception>
    public static RoomTypeId From(Guid value)
    {
        return new RoomTypeId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
