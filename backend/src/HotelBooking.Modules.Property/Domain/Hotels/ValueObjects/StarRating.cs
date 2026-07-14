namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

/// <summary>Represents an official hotel star rating from one through five.</summary>
public sealed record StarRating
{
    /// <summary>Gets the numeric star rating.</summary>
    public int Value { get; }

    private StarRating(int value)
    {
        Value = value;
    }

    /// <summary>Creates a valid one-to-five star hotel rating.</summary>
    public static StarRating Create(int value)
    {
        if (value is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Star rating must be between 1 and 5.");
        }

        return new StarRating(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Value}-star";
    }
}
