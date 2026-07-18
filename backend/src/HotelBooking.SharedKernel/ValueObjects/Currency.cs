using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.SharedKernel.ValueObjects;

/// <summary>
/// Represents the denomination of a hotel price or payment amount.
/// </summary>
public sealed record Currency
{
    private Currency(string code)
    {
        Code = code;
    }

    /// <summary>
    /// Gets the canonical three-letter currency code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Creates a currency from a code, normalized without regard to letter case.
    /// </summary>
    /// <param name="code">The three-letter currency code.</param>
    /// <returns>The currency identified by the canonical uppercase code.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the code is missing or is not exactly three ASCII letters.
    /// </exception>
    public static Currency FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainArgumentException("Currency code is required.", nameof(code));
        }

        string trimmedCode = code.Trim();

        if (trimmedCode.Length != 3 ||
            trimmedCode.Any(character => !char.IsAsciiLetter(character)))
        {
            throw new DomainArgumentException(
                "Currency code must contain exactly three ASCII letters.",
                nameof(code));
        }

        return new Currency(trimmedCode.ToUpperInvariant());
    }
}
