using HotelBooking.SharedKernel.Exceptions;
using GuestId = HotelBooking.Modules.Booking.Domain.References.GuestId;
using HotelId = HotelBooking.Modules.Booking.Domain.References.HotelId;
using PriceQuoteId = HotelBooking.Modules.Booking.Domain.References.PriceQuoteId;
using RatePlanId = HotelBooking.Modules.Booking.Domain.References.RatePlanId;
using RoomTypeId = HotelBooking.Modules.Booking.Domain.References.RoomTypeId;

namespace HotelBooking.UnitTests.Booking.References;

/// <summary>Proves creation rules for Booking-local references to identifiers owned by other modules.</summary>
public class ReferenceIdTests
{
    private static readonly Guid ExistingId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    [Fact]
    public void GuestId_reconstructs_a_non_empty_identity_reference()
    {
        GuestId id = GuestId.From(ExistingId);

        Assert.Equal(ExistingId, id.Value);
        Assert.Equal(ExistingId.ToString(), id.ToString());
    }

    [Fact]
    public void GuestId_rejects_an_empty_identity_reference()
    {
        Assert.Throws<DomainArgumentException>(() => GuestId.From(Guid.Empty));
    }

    [Fact]
    public void HotelId_reconstructs_a_non_empty_property_reference()
    {
        HotelId id = HotelId.From(ExistingId);

        Assert.Equal(ExistingId, id.Value);
    }

    [Fact]
    public void HotelId_rejects_an_empty_property_reference()
    {
        Assert.Throws<DomainArgumentException>(() => HotelId.From(Guid.Empty));
    }

    [Fact]
    public void RoomTypeId_reconstructs_a_non_empty_property_reference()
    {
        RoomTypeId id = RoomTypeId.From(ExistingId);

        Assert.Equal(ExistingId, id.Value);
    }

    [Fact]
    public void RoomTypeId_rejects_an_empty_property_reference()
    {
        Assert.Throws<DomainArgumentException>(() => RoomTypeId.From(Guid.Empty));
    }

    [Fact]
    public void RatePlanId_reconstructs_a_non_empty_pricing_reference()
    {
        RatePlanId id = RatePlanId.From(ExistingId);

        Assert.Equal(ExistingId, id.Value);
    }

    [Fact]
    public void RatePlanId_rejects_an_empty_pricing_reference()
    {
        Assert.Throws<DomainArgumentException>(() => RatePlanId.From(Guid.Empty));
    }

    [Fact]
    public void PriceQuoteId_reconstructs_a_non_empty_pricing_reference()
    {
        PriceQuoteId id = PriceQuoteId.From(ExistingId);

        Assert.Equal(ExistingId, id.Value);
    }

    [Fact]
    public void PriceQuoteId_rejects_an_empty_pricing_reference()
    {
        Assert.Throws<DomainArgumentException>(() => PriceQuoteId.From(Guid.Empty));
    }
}
