using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.Modules.Property.Domain.Hotels.Events;
using HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.Property.Hotels;

public class HotelTests
{
    private static readonly DateTimeOffset UpdatedAt =
        new(2026, 7, 1, 11, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateDraft_creates_hotel_in_draft_status()
    {
        var hotelId = HotelId.Create();
        DateTimeOffset createdAt = new(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);

        var hotel = Hotel.CreateDraft(
            id: hotelId,
            name: "  Hoon Hotel  ",
            slug: "   Hoon-Hotel",
            starRating: StarRating.Create(4),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("krw"),
            defaultLanguage: "ko",
            createdAt: createdAt);

        Assert.Equal(hotelId, hotel.Id);
        Assert.Equal("Hoon Hotel", hotel.Name);
        Assert.Equal("hoon-hotel", hotel.Slug);
        Assert.Equal(HotelStatus.Draft, hotel.Status);
        Assert.Equal(StarRating.Create(4), hotel.StarRating);
        Assert.Equal("Asia/Seoul", hotel.TimeZoneId);
        Assert.Equal(Currency.FromCode("KRW"), hotel.SellingCurrency);
        Assert.Equal("ko", hotel.DefaultLanguage);
        Assert.Equal(createdAt, hotel.CreatedAt);
    }

    [Fact]
    public void CreateDraft_throws_when_id_is_empty()
    {
        DomainArgumentException domainArgumentException =
            Assert.Throws<DomainArgumentException>(() => CreateDraftHotel(default));

        Assert.Equal("id", domainArgumentException.ParamName);
    }

    [Fact]
    public void CreateDraft_throws_when_name_is_empty()
    {
        Assert.Throws<DomainArgumentException>(() => Hotel.CreateDraft(
            id: HotelId.Create(),
            name: "",
            slug: "   Hoon-Hotel",
            starRating: StarRating.Create(4),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("krw"),
            defaultLanguage: "ko",
            createdAt: new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void CreateDraft_throws_when_name_length_longer_than_100()
    {
        Assert.Throws<DomainArgumentException>(() => Hotel.CreateDraft(
            id: HotelId.Create(),
            name: new string('a', 101),
            slug: "   Hoon-Hotel",
            starRating: StarRating.Create(4),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("krw"),
            defaultLanguage: "ko",
            createdAt: new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void CreateDraft_throws_when_slug_is_empty()
    {
        Assert.Throws<DomainArgumentException>(() => Hotel.CreateDraft(
            id: HotelId.Create(),
            name: "  Hoon Hotel  ",
            slug: "",
            starRating: StarRating.Create(4),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("krw"),
            defaultLanguage: "ko",
            createdAt: new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void CreateDraft_throws_when_slug_length_longer_than_200()
    {
        Assert.Throws<DomainArgumentException>(() => Hotel.CreateDraft(
            id: HotelId.Create(),
            name: "  Hoon Hotel  ",
            slug: new string('a', 201),
            starRating: StarRating.Create(4),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("krw"),
            defaultLanguage: "ko",
            createdAt: new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void CreateDraft_rejects_missing_selling_currency()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() => Hotel.CreateDraft(
            id: HotelId.Create(),
            name: " Hoon Hotel ",
            slug: "   Hoon-Hotel",
            starRating: StarRating.Create(4),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: null!,
            defaultLanguage: "ko",
            createdAt: new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("sellingCurrency", exception.ParamName);
    }

    [Fact]
    public void CreateDraft_throws_when_time_zone_id_is_empty()
    {
        Assert.Throws<DomainArgumentException>(() => Hotel.CreateDraft(
            id: HotelId.Create(),
            name: " Hoon Hotel ",
            slug: "   Hoon-Hotel",
            starRating: StarRating.Create(4),
            timeZoneId: "",
            sellingCurrency: Currency.FromCode("krw"),
            defaultLanguage: "ko",
            createdAt: new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void CreateDraft_throws_when_default_language_is_empty()
    {
        Assert.Throws<DomainArgumentException>(() => Hotel.CreateDraft(
            id: HotelId.Create(),
            name: " Hoon Hotel ",
            slug: "   Hoon-Hotel",
            starRating: StarRating.Create(4),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("krw"),
            defaultLanguage: "",
            createdAt: new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void SubmitForReview_changes_status_when_hotel_is_ready_to_review()
    {
        Hotel hotel = CreateReadyDraftHotel();

        hotel.SubmitForReview();

        Assert.Equal(HotelStatus.PendingReview, hotel.Status);
    }

    [Fact]
    public void SubmitForReview_throws_when_address_is_not_set()
    {
        Hotel hotel = CreateDraftHotel();

        hotel.UpdateContactInfo(new ContactInfo(
            Phone: "010-1234-5678",
            Email: null,
            Website: null), UpdatedAt);

        hotel.UpdateCheckInPolicy(new CheckInPolicy(
            CheckInFrom: new TimeOnly(15, 0, 0),
            CheckInUntil: new TimeOnly(23, 0, 0),
            CheckOutUntil: new TimeOnly(11, 0, 0),
            AllowsEarlyCheckIn: true,
            AllowsLateCheckOut: true), UpdatedAt);

        hotel.UpdateHotelPolicy(new HotelPolicy(
            AllowsSmoking: false,
            AllowsPets: true,
            AllowsChildren: true,
            MinimumCheckInAge: 18,
            RequiresDeposit: false), UpdatedAt);

        Assert.Throws<DomainArgumentException>(hotel.SubmitForReview);

        Assert.Equal(HotelStatus.Draft, hotel.Status);
    }

    [Fact]
    public void SubmitForReview_throws_when_contact_info_is_not_set()
    {
        Hotel hotel = CreateDraftHotel();

        hotel.UpdateLocation(
            address: new Address(
                CountryCode: "KR",
                City: "Seoul",
                PostalCode: "12345",
                Region: "Seoul",
                StreetAddress: "LeeSunsin",
                DetailAddress: null),
            geoLocation: null,
            updatedAt: UpdatedAt);

        hotel.UpdateCheckInPolicy(new CheckInPolicy(
            CheckInFrom: new TimeOnly(15, 0, 0),
            CheckInUntil: new TimeOnly(23, 0, 0),
            CheckOutUntil: new TimeOnly(11, 0, 0),
            AllowsEarlyCheckIn: true,
            AllowsLateCheckOut: true), UpdatedAt);

        hotel.UpdateHotelPolicy(new HotelPolicy(
            AllowsSmoking: false,
            AllowsPets: true,
            AllowsChildren: true,
            MinimumCheckInAge: 18,
            RequiresDeposit: false), UpdatedAt);

        Assert.Throws<DomainArgumentException>(hotel.SubmitForReview);

        Assert.Equal(HotelStatus.Draft, hotel.Status);
    }

    [Fact]
    public void SubmitForReview_throws_when_check_in_policy_is_not_set()
    {
        Hotel hotel = CreateDraftHotel();

        hotel.UpdateLocation(
            address: new Address(
                CountryCode: "KR",
                City: "Seoul",
                PostalCode: "12345",
                Region: "Seoul",
                StreetAddress: "LeeSunsin",
                DetailAddress: null),
            geoLocation: null,
            updatedAt: UpdatedAt);

        hotel.UpdateContactInfo(new ContactInfo(
            Phone: "010-1234-5678",
            Email: null,
            Website: null), UpdatedAt);

        hotel.UpdateHotelPolicy(new HotelPolicy(
            AllowsSmoking: false,
            AllowsPets: true,
            AllowsChildren: true,
            MinimumCheckInAge: 18,
            RequiresDeposit: false), UpdatedAt);

        Assert.Throws<DomainArgumentException>(hotel.SubmitForReview);

        Assert.Equal(HotelStatus.Draft, hotel.Status);
    }

    [Fact]
    public void SubmitForReview_throws_when_hotel_policy_is_not_set()
    {
        Hotel hotel = CreateDraftHotel();

        hotel.UpdateLocation(
            address: new Address(
                CountryCode: "KR",
                City: "Seoul",
                PostalCode: "12345",
                Region: "Seoul",
                StreetAddress: "LeeSunsin",
                DetailAddress: null),
            geoLocation: null,
            updatedAt: UpdatedAt);

        hotel.UpdateContactInfo(new ContactInfo(
            Phone: "010-1234-5678",
            Email: null,
            Website: null), UpdatedAt);

        hotel.UpdateCheckInPolicy(new CheckInPolicy(
            CheckInFrom: new TimeOnly(15, 0, 0),
            CheckInUntil: new TimeOnly(23, 0, 0),
            CheckOutUntil: new TimeOnly(11, 0, 0),
            AllowsEarlyCheckIn: true,
            AllowsLateCheckOut: true), UpdatedAt);

        Assert.Throws<DomainArgumentException>(hotel.SubmitForReview);

        Assert.Equal(HotelStatus.Draft, hotel.Status);
    }

    [Fact]
    public void Publish_throws_when_hotel_is_not_pending_review()
    {
        Hotel hotel = CreateDraftHotel();

        Assert.Throws<DomainException>(() => hotel.Publish(UpdatedAt));
    }

    [Fact]
    public void Publish_changes_status_when_hotel_is_ready_to_publish()
    {
        Hotel hotel = CreateReadyDraftHotel();

        hotel.SubmitForReview();

        hotel.Publish(UpdatedAt);

        Assert.Equal(HotelStatus.Active, hotel.Status);
    }

    [Fact]
    public void Publish_raises_hotel_published_domain_event()
    {
        Hotel hotel = CreateReadyDraftHotel();

        hotel.SubmitForReview();

        DateTimeOffset publishedAt = new(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);

        hotel.Publish(publishedAt);

        HotelPublishedDomainEvent domainEvent = Assert.Single(hotel.DomainEvents.OfType<HotelPublishedDomainEvent>());

        Assert.Equal(hotel.Id, domainEvent.HotelId);
        Assert.Equal(publishedAt, domainEvent.OccurredAt);
    }

    [Fact]
    public void Publish_does_not_raise_event_when_hotel_is_not_pending_review()
    {
        Hotel hotel = CreateDraftHotel();

        Assert.Throws<DomainException>(
            () => hotel.Publish(
                new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.DoesNotContain(
            hotel.DomainEvents,
            domainEvent => domainEvent is HotelPublishedDomainEvent);
    }

    [Fact]
    public void Suspend_changes_status_when_hotel_is_active()
    {
        Hotel hotel = CreateReadyDraftHotel();

        hotel.SubmitForReview();

        hotel.Publish(UpdatedAt);

        hotel.Suspend();

        Assert.Equal(HotelStatus.Suspended, hotel.Status);
    }

    [Fact]
    public void Suspend_throws_when_hotel_is_not_active()
    {
        Hotel hotel = CreateDraftHotel();

        Assert.Throws<DomainException>(hotel.Suspend);
    }

    [Fact]
    public void Close_changes_status_when_hotel_is_not_closed()
    {
        Hotel hotel = CreateReadyDraftHotel();

        hotel.SubmitForReview();

        hotel.Publish(UpdatedAt);

        hotel.Close();

        Assert.Equal(HotelStatus.Closed, hotel.Status);
    }

    [Fact]
    public void UpdateBasicInfo_throws_when_hotel_is_closed()
    {
        Hotel hotel = CreateReadyDraftHotel();

        hotel.SubmitForReview();

        hotel.Publish(UpdatedAt);

        hotel.Close();

        Assert.Throws<DomainException>(
            () => hotel.UpdateBasicInfo(
                name: "New Hoon Hotel",
                slug: "new-hoon-hotel",
                starRating: null,
                updatedAt: UpdatedAt));
    }

    [Fact]
    public void UpdateBasicInfo_records_the_explicit_update_timestamp()
    {
        Hotel hotel = CreateDraftHotel();

        hotel.UpdateBasicInfo(
            name: "New Hoon Hotel",
            slug: "new-hoon-hotel",
            starRating: StarRating.Create(5),
            updatedAt: UpdatedAt);

        Assert.Equal("New Hoon Hotel", hotel.Name);
        Assert.Equal("new-hoon-hotel", hotel.Slug);
        Assert.Equal(StarRating.Create(5), hotel.StarRating);
        Assert.Equal(UpdatedAt, hotel.UpdatedAt);
    }

    [Fact]
    public void Reactivate_changes_status_when_hotel_is_suspended()
    {
        Hotel hotel = CreateReadyDraftHotel();

        hotel.SubmitForReview();

        hotel.Publish(UpdatedAt);

        hotel.Suspend();

        hotel.Reactivate();

        Assert.Equal(HotelStatus.Active, hotel.Status);
    }

    [Fact]
    public void Reactivate_throws_when_hotel_is_not_suspended()
    {
        Hotel hotel = CreateDraftHotel();

        Assert.Throws<DomainException>(hotel.Reactivate);
    }

    private static Hotel CreateDraftHotel()
    {
        return CreateDraftHotel(HotelId.Create());
    }

    private static Hotel CreateDraftHotel(HotelId id)
    {
        return Hotel.CreateDraft(
            id: id,
            name: " Hoon Hotel ",
            slug: "   Hoon-Hotel",
            starRating: StarRating.Create(4),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("krw"),
            defaultLanguage: "ko",
            createdAt: new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero));
    }

    private static Hotel CreateReadyDraftHotel()
    {
        Hotel hotel = CreateDraftHotel();

        hotel.UpdateCheckInPolicy(new CheckInPolicy(
            CheckInFrom: new TimeOnly(15, 0, 0),
            CheckInUntil: new TimeOnly(23, 0, 0),
            CheckOutUntil: new TimeOnly(11, 0, 0),
            AllowsEarlyCheckIn: true,
            AllowsLateCheckOut: true), UpdatedAt);

        hotel.UpdateContactInfo(new ContactInfo(
            Phone: "010-1234-5678",
            Email: null,
            Website: null), UpdatedAt);

        hotel.UpdateHotelPolicy(new HotelPolicy(
            AllowsSmoking: false,
            AllowsPets: true,
            AllowsChildren: true,
            MinimumCheckInAge: 18,
            RequiresDeposit: false), UpdatedAt);

        hotel.UpdateLocation(
            address: new Address(
                CountryCode: "KR",
                City: "Seoul",
                PostalCode: "12345",
                Region: "Seoul",
                StreetAddress: "LeeSunsin",
                DetailAddress: null),
            geoLocation: null,
            updatedAt: UpdatedAt);

        return hotel;
    }
}
