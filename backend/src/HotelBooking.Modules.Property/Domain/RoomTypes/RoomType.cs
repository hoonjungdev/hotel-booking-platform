using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.RoomTypes;

public sealed class RoomType : AggregateRoot<RoomTypeId>
{

    private readonly List<BedComposition> _bedCompositions = [];

    public HotelId HotelId { get; private set; }
    public string Name { get; private set; } = null!;
    public RoomTypeCode Code { get; private set; } = null!;
    public RoomTypeStatus Status { get; private set; }
    public Occupancy Occupancy { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public IReadOnlyList<BedComposition> BedCompositions => _bedCompositions.AsReadOnly();
    public bool IsSellable => Status == RoomTypeStatus.Active;

    private RoomType()
    {
    }

    private RoomType(
        RoomTypeId id,
        HotelId hotelId,
        string name,
        RoomTypeCode code,
        RoomTypeStatus status,
        Occupancy occupancy,
        IReadOnlyCollection<BedComposition> bedCompositions,
        DateTimeOffset createdAt)
    {
        Id = id;
        HotelId = hotelId;
        Name = name;
        Code = code;
        Status = status;
        Occupancy = occupancy;
        _bedCompositions = bedCompositions.ToList();
        CreatedAt = createdAt;
    }

    public static RoomType CreateDraft(
        RoomTypeId id,
        HotelId hotelId,
        string name,
        RoomTypeCode code,
        Occupancy occupancy,
        IReadOnlyCollection<BedComposition> bedCompositions,
        DateTimeOffset createdAt)
    {
        if (id == default)
        {
            throw new DomainArgumentException("Room type ID is required.", nameof(id));
        }

        if (hotelId == default)
        {
            throw new DomainArgumentException("Hotel ID is required.", nameof(hotelId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainArgumentException("Name is required.", nameof(name));
        }

        string trimmedName = name.Trim();

        if (trimmedName.Length > 100)
        {
            throw new DomainArgumentException("Name cannot be longer than 100 characters.", nameof(name));
        }

        if (code == null)
        {
            throw new DomainArgumentException("Code is required.", nameof(code));
        }

        if (occupancy == null)
        {
            throw new DomainArgumentException("Occupancy is required.", nameof(occupancy));
        }

        if (bedCompositions == null)
        {
            throw new DomainArgumentException("Bed compositions are required.", nameof(bedCompositions));
        }
        if (bedCompositions.Count == 0)
        {
            throw new DomainArgumentException("At least one bed composition is required.", nameof(bedCompositions));
        }
        if (bedCompositions.Any(bedComposition => bedComposition is null))
        {
            throw new DomainArgumentException("Bed compositions cannot contain null values.", nameof(bedCompositions));
        }

        return new RoomType(
            id,
            hotelId,
            trimmedName,
            code,
            RoomTypeStatus.Draft,
            occupancy,
            bedCompositions,
            createdAt);
    }

    public void Activate()
    {
        if (Status != RoomTypeStatus.Draft)
        {
            throw new DomainException("Room type cannot be activated from the current status. Current status: " + Status);
        }

        Status = RoomTypeStatus.Active;
    }

    public void Suspend()
    {
        if (Status != RoomTypeStatus.Active)
        {
            throw new DomainException("Room type cannot be suspended from the current status. Current status: " + Status);
        }

        Status = RoomTypeStatus.Suspended;
    }

    public void Close()
    {
        if (Status == RoomTypeStatus.Closed)
        {
            return;
        }

        Status = RoomTypeStatus.Closed;
    }

    public void Reactivate()
    {
        if (Status != RoomTypeStatus.Suspended)
        {
            throw new DomainException("Room type cannot be reactivated from the current status. Current status: " + Status);
        }

        Status = RoomTypeStatus.Active;
    }

    public bool CanAccommodate(int adults, int children)
    {
        return Occupancy.CanAccommodate(adults, children);
    }
}
