using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;

/// <summary>Represents a normalized hotel-specific room type code.</summary>
public sealed record RoomTypeCode
{
    /// <summary>Gets the trimmed uppercase code.</summary>
    public string Value { get; }

    private RoomTypeCode(string value)
    {
        Value = value;
    }

    /// <summary>Creates a normalized room type code no longer than 30 characters.</summary>
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

    /// <inheritdoc />
    public override string ToString() => Value;
}
