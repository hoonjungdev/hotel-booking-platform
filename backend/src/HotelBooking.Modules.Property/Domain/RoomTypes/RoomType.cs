using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.RoomTypes;

/// <summary>
/// Represents a guest-bookable category of rooms offered by one hotel.
/// </summary>
public sealed class RoomType : AggregateRoot<RoomTypeId>
{

    private readonly List<BedComposition> _bedCompositions = [];

    /// <summary>Gets the hotel that offers this room type.</summary>
    public HotelId HotelId { get; private set; }
    /// <summary>Gets the guest-facing room type name.</summary>
    public string Name { get; private set; } = null!;
    /// <summary>Gets the hotel-specific room type code.</summary>
    public RoomTypeCode Code { get; private set; } = null!;
    /// <summary>Gets the room type lifecycle status.</summary>
    public RoomTypeStatus Status { get; private set; }
    /// <summary>Gets the supported guest occupancy.</summary>
    public Occupancy Occupancy { get; private set; } = null!;
    /// <summary>Gets the explicit creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; private set; }
    /// <summary>Gets the latest update timestamp when later editing is introduced.</summary>
    public DateTimeOffset? UpdatedAt { get; private set; }
    /// <summary>Gets the bed configuration offered by this room type.</summary>
    public IReadOnlyList<BedComposition> BedCompositions => _bedCompositions.AsReadOnly();
    /// <summary>Gets whether the room type is currently available for sale.</summary>
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

    /// <summary>Creates a draft room type that is not yet sellable.</summary>
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

    /// <summary>Activates a fully configured draft room type for sale.</summary>
    public void Activate()
    {
        if (Status != RoomTypeStatus.Draft)
        {
            throw new DomainException("Room type cannot be activated from the current status. Current status: " + Status);
        }

        Status = RoomTypeStatus.Active;
    }

    /// <summary>Temporarily removes an active room type from sale.</summary>
    public void Suspend()
    {
        if (Status != RoomTypeStatus.Active)
        {
            throw new DomainException("Room type cannot be suspended from the current status. Current status: " + Status);
        }

        Status = RoomTypeStatus.Suspended;
    }

    /// <summary>Permanently closes the room type; repeated closure is idempotent.</summary>
    public void Close()
    {
        if (Status == RoomTypeStatus.Closed)
        {
            return;
        }

        Status = RoomTypeStatus.Closed;
    }

    /// <summary>Returns a suspended room type to active sale.</summary>
    public void Reactivate()
    {
        if (Status != RoomTypeStatus.Suspended)
        {
            throw new DomainException("Room type cannot be reactivated from the current status. Current status: " + Status);
        }

        Status = RoomTypeStatus.Active;
    }

    /// <summary>Determines whether an adult and child guest composition fits this room type.</summary>
    public bool CanAccommodate(int adults, int children)
    {
        return Occupancy.CanAccommodate(adults, children);
    }
}
