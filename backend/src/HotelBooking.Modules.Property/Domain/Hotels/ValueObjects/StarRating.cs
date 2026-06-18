namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

public sealed record StarRating
{
    public int Value { get; }

    private StarRating(int value)
    {
        Value = value;
    }

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

    public override string ToString()
    {
        return $"{Value}-star";
    }
}
