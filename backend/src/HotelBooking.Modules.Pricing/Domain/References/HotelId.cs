using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.References;

/// <summary>References the hotel that offers a rate plan without depending on the Property module.</summary>
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

    /// <summary>Reconstructs a hotel reference from an identifier owned by Property.</summary>
    /// <param name="value">The existing non-empty Hotel identifier.</param>
    /// <returns>A Pricing-local reference to the Hotel.</returns>
    /// <exception cref="DomainArgumentException">Thrown when the identifier is empty.</exception>
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
