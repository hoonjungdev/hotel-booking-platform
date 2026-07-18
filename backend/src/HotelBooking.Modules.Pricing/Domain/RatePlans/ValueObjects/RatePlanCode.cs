using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;

/// <summary>Represents a canonical, case-insensitive hotel-specific RatePlan code.</summary>
public sealed record RatePlanCode
{
    /// <summary>Gets the trimmed code in canonical uppercase form.</summary>
    public string Value { get; }

    private RatePlanCode(string value)
    {
        Value = value;
    }

    /// <summary>Creates a required RatePlan code in canonical uppercase form.</summary>
    /// <param name="code">The hotel-defined operational identifier.</param>
    /// <returns>A trimmed, case-normalized RatePlan code.</returns>
    /// <exception cref="DomainArgumentException">Thrown when the code is missing.</exception>
    public static RatePlanCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainArgumentException("Rate plan code is required.", nameof(code));
        }

        string normalizedCode = code.Trim().ToUpperInvariant();

        return new RatePlanCode(normalizedCode);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}
