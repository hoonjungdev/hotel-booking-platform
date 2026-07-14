using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.References;

public readonly record struct ReservationId
{
    public Guid Value { get; }

    private ReservationId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("ReservationId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static ReservationId Create()
    {
        return new ReservationId(Guid.NewGuid());
    }

    public static ReservationId From(Guid value)
    {
        return new ReservationId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
