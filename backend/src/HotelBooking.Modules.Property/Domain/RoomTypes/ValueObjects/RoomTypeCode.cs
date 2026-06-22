using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;

public sealed record RoomTypeCode
{
    public string Value { get; }

    private RoomTypeCode(string value)
    {
        Value = value;
    }

    public static RoomTypeCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainArgumentException("Room type code is required.", nameof(code));
        }

        string trimmedCode = code.Trim();

        if (trimmedCode.Length > 30)
        {
            throw new DomainArgumentException("Room type code cannot be longer than 30 characters.", nameof(code));
        }

        return new RoomTypeCode(trimmedCode.ToUpperInvariant());
    }

    public override string ToString() => Value;
}
