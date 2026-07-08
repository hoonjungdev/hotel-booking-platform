using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.References;

public readonly record struct HotelId
{
    public Guid Value { get; }

    private HotelId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("HotelId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static HotelId Create()
    {
        return new HotelId(Guid.NewGuid());
    }

    public static HotelId From(Guid value)
    {
        return new HotelId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
