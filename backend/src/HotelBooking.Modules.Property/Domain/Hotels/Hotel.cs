using HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.Hotels;

public sealed class Hotel
{
    public HotelId Id { get; private set; }
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public HotelStatus Status { get; private set; }

    public StarRating? StarRating { get; private set; }

    public Address? Address { get; private set; }
    public GeoLocation? GeoLocation { get; private set; }
    public ContactInfo? ContactInfo { get; private set; }

    public string TimeZoneId { get; private set; } = null!;
    public string DefaultCurrency { get; private set; } = null!;
    public string DefaultLanguage { get; private set; } = null!;

    public CheckInPolicy? CheckInPolicy { get; private set; }
    public HotelPolicy? HotelPolicy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Hotel()
    {
        // Required by EF Core
    }

    private Hotel(
        HotelId id,
        string name,
        string slug,
        HotelStatus status,
        StarRating? starRating,
        string timeZoneId,
        string defaultCurrency,
        string defaultLanguage,
        DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Slug = slug;
        Status = status;
        StarRating = starRating;
        TimeZoneId = timeZoneId;
        DefaultCurrency = defaultCurrency;
        DefaultLanguage = defaultLanguage;
        CreatedAt = createdAt;
    }

    public static Hotel CreateDraft(
        HotelId id,
        string name,
        string slug,
        StarRating? starRating,
        string timeZoneId,
        string defaultCurrency,
        string defaultLanguage,
        DateTimeOffset createdAt)
    {
        ValidateHotelId(id);
        string normalizedName = ValidateName(name);
        string normalizedSlug = ValidateSlug(slug);
        ValidateTimeZoneId(timeZoneId);
        string normalizedDefaultCurrency = ValidateCurrency(defaultCurrency);
        ValidateLanguage(defaultLanguage);

        return new Hotel(
            id,
            normalizedName,
            normalizedSlug,
            HotelStatus.Draft,
            starRating,
            timeZoneId,
            normalizedDefaultCurrency,
            defaultLanguage,
            createdAt);
    }

    public void SubmitForReview()
    {
        if (Status != HotelStatus.Draft)
        {
            throw new DomainException($"Hotel cannot be submitted for review from the current status. Current status: {Status}");
        }

        ValidatePublished();

        Status = HotelStatus.PendingReview;
    }

    public void Publish()
    {
        if (Status != HotelStatus.PendingReview)
        {
            throw new DomainException($"Hotel cannot be published from the current status. Current status: {Status}");
        }

        ValidatePublished();

        Status = HotelStatus.Active;
    }

    public void Suspend()
    {
        if (Status != HotelStatus.Active)
        {
            throw new DomainException($"Hotel cannot be suspended from the current status. Current status: {Status}");
        }

        Status = HotelStatus.Suspended;
    }

    public void Reactivate()
    {
        if (Status != HotelStatus.Suspended)
        {
            throw new DomainException($"Hotel cannot be reactivated from the current status. Current status: {Status}");
        }

        Status = HotelStatus.Active;
    }

    public void Close()
    {
        if (Status == HotelStatus.Closed)
        {
            return;
        }

        Status = HotelStatus.Closed;
    }

    public void UpdateBasicInfo(
        string name,
        string slug,
        StarRating? starRating)
    {
        EnsureEditable();

        string normalizedName = ValidateName(name);
        string normalizedSlug = ValidateSlug(slug);

        Name = normalizedName;
        Slug = normalizedSlug;
        StarRating = starRating;

        Touch();
    }

    public void UpdateLocation(
        Address address,
        GeoLocation? geoLocation)
    {
        EnsureEditable();

        Address = address;
        GeoLocation = geoLocation;

        Touch();
    }

    public void UpdateContactInfo(ContactInfo contactInfo)
    {
        EnsureEditable();

        ContactInfo = contactInfo;

        Touch();
    }

    public void UpdateCheckInPolicy(CheckInPolicy checkInPolicy)
    {
        EnsureEditable();

        CheckInPolicy = checkInPolicy;

        Touch();
    }

    public void UpdateHotelPolicy(HotelPolicy hotelPolicy)
    {
        EnsureEditable();

        HotelPolicy = hotelPolicy;

        Touch();
    }

    private void EnsureEditable()
    {
        if (Status == HotelStatus.Closed)
        {
            throw new DomainException("Closed hotels cannot be edited.");
        }
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void ValidatePublished()
    {
        if (Address is null)
        {
            throw new DomainArgumentException("Hotel address is required before publishing.", nameof(Address));
        }

        if (ContactInfo is null)
        {
            throw new DomainArgumentException("Hotel contact information is required before publishing.", nameof(ContactInfo));
        }

        if (CheckInPolicy is null)
        {
            throw new DomainArgumentException("Hotel check-in policy is required before publishing.", nameof(CheckInPolicy));
        }

        if (HotelPolicy is null)
        {
            throw new DomainArgumentException("Hotel policy is required before publishing.", nameof(HotelPolicy));
        }
    }

    private static void ValidateHotelId(HotelId id)
    {
        if (id == default)
        {
            throw new DomainArgumentException("Hotel ID is required", nameof(id));
        }
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainArgumentException("Hotel name is required", nameof(name));
        }

        string normalizedName = name.Trim();

        if (normalizedName.Length > 100)
        {
            throw new DomainArgumentException("Hotel name must be 100 characters or less.", nameof(name));
        }

        return normalizedName;
    }

    private static string ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainArgumentException("Hotel slug is required", nameof(slug));
        }

        string normalizedSlug = slug.Trim().ToLowerInvariant();

        if (normalizedSlug.Length > 200)
        {
            throw new DomainArgumentException("Hotel slug must be 200 characters or less.", nameof(slug));
        }

        return normalizedSlug;
    }

    private static void ValidateTimeZoneId(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new DomainArgumentException("Time zone ID is required", nameof(timeZoneId));
        }
    }

    private static string ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainArgumentException("Default currency is required", nameof(currency));
        }

        string normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != 3)
        {
            throw new DomainArgumentException("Default currency must be an ISO 4217 currency code.", nameof(currency));
        }

        return normalizedCurrency;
    }

    private static void ValidateLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            throw new DomainArgumentException("Default language is required", nameof(language));
        }
    }
}
