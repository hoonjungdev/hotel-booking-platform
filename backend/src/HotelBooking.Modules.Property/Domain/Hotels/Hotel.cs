using HotelBooking.Modules.Property.Domain.Hotels.Events;
using HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.Hotels;

/// <summary>
/// Represents a hotel and governs the information and lifecycle required before it can be published.
/// </summary>
public sealed class Hotel : AggregateRoot<HotelId>
{
    /// <summary>Gets the guest-facing hotel name.</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Gets the normalized URL slug.</summary>
    public string Slug { get; private set; } = null!;

    /// <summary>Gets the hotel's operational lifecycle status.</summary>
    public HotelStatus Status { get; private set; }

    /// <summary>Gets the optional official star rating.</summary>
    public StarRating? StarRating { get; private set; }

    /// <summary>Gets the hotel's postal address.</summary>
    public Address? Address { get; private set; }
    /// <summary>Gets the optional geographic coordinates.</summary>
    public GeoLocation? GeoLocation { get; private set; }
    /// <summary>Gets the hotel's guest contact information.</summary>
    public ContactInfo? ContactInfo { get; private set; }

    /// <summary>Gets the IANA time-zone identifier used for hotel-local operations.</summary>
    public string TimeZoneId { get; private set; } = null!;
    /// <summary>Gets the ISO 4217 default currency code.</summary>
    public string DefaultCurrency { get; private set; } = null!;
    /// <summary>Gets the default language used by the hotel.</summary>
    public string DefaultLanguage { get; private set; } = null!;

    /// <summary>Gets the hotel's check-in and check-out policy.</summary>
    public CheckInPolicy? CheckInPolicy { get; private set; }
    /// <summary>Gets the hotel's guest and deposit policy.</summary>
    public HotelPolicy? HotelPolicy { get; private set; }

    /// <summary>Gets the explicit creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; private set; }
    /// <summary>Gets the timestamp of the latest information update or publication.</summary>
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

    /// <summary>
    /// Creates a draft hotel that can be enriched before review and publication.
    /// </summary>
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

    /// <summary>
    /// Submits a complete draft hotel for review before publication.
    /// </summary>
    public void SubmitForReview()
    {
        if (Status != HotelStatus.Draft)
        {
            throw new DomainException($"Hotel cannot be submitted for review from the current status. Current status: {Status}");
        }

        ValidatePublished();

        Status = HotelStatus.PendingReview;
    }

    /// <summary>
    /// Publishes an approved hotel and records the publication domain event.
    /// </summary>
    /// <param name="publishedAt">The explicit timestamp supplied by the application layer.</param>
    public void Publish(DateTimeOffset publishedAt)
    {
        if (Status != HotelStatus.PendingReview)
        {
            throw new DomainException($"Hotel cannot be published from the current status. Current status: {Status}");
        }

        ValidatePublished();

        Status = HotelStatus.Active;
        UpdatedAt = publishedAt;

        RaiseDomainEvent(new HotelPublishedDomainEvent(Id, publishedAt));
    }

    /// <summary>
    /// Suspends an active hotel so it is no longer operationally available.
    /// </summary>
    public void Suspend()
    {
        if (Status != HotelStatus.Active)
        {
            throw new DomainException($"Hotel cannot be suspended from the current status. Current status: {Status}");
        }

        Status = HotelStatus.Suspended;
    }

    /// <summary>
    /// Returns a suspended hotel to active operation.
    /// </summary>
    public void Reactivate()
    {
        if (Status != HotelStatus.Suspended)
        {
            throw new DomainException($"Hotel cannot be reactivated from the current status. Current status: {Status}");
        }

        Status = HotelStatus.Active;
    }

    /// <summary>
    /// Permanently closes the hotel; repeated closure is idempotent.
    /// </summary>
    public void Close()
    {
        if (Status == HotelStatus.Closed)
        {
            return;
        }

        Status = HotelStatus.Closed;
    }

    /// <summary>
    /// Updates the editable guest-facing identity of a non-closed hotel.
    /// </summary>
    public void UpdateBasicInfo(
        string name,
        string slug,
        StarRating? starRating,
        DateTimeOffset updatedAt)
    {
        EnsureEditable();

        string normalizedName = ValidateName(name);
        string normalizedSlug = ValidateSlug(slug);

        Name = normalizedName;
        Slug = normalizedSlug;
        StarRating = starRating;

        Touch(updatedAt);
    }

    /// <summary>
    /// Updates the address and optional coordinates of a non-closed hotel.
    /// </summary>
    public void UpdateLocation(
        Address address,
        GeoLocation? geoLocation,
        DateTimeOffset updatedAt)
    {
        EnsureEditable();

        Address = address;
        GeoLocation = geoLocation;

        Touch(updatedAt);
    }

    /// <summary>
    /// Updates the guest contact information of a non-closed hotel.
    /// </summary>
    public void UpdateContactInfo(ContactInfo contactInfo, DateTimeOffset updatedAt)
    {
        EnsureEditable();

        ContactInfo = contactInfo;

        Touch(updatedAt);
    }

    /// <summary>
    /// Updates check-in and check-out rules for a non-closed hotel.
    /// </summary>
    public void UpdateCheckInPolicy(CheckInPolicy checkInPolicy, DateTimeOffset updatedAt)
    {
        EnsureEditable();

        CheckInPolicy = checkInPolicy;

        Touch(updatedAt);
    }

    /// <summary>
    /// Updates guest eligibility and deposit rules for a non-closed hotel.
    /// </summary>
    public void UpdateHotelPolicy(HotelPolicy hotelPolicy, DateTimeOffset updatedAt)
    {
        EnsureEditable();

        HotelPolicy = hotelPolicy;

        Touch(updatedAt);
    }

    private void EnsureEditable()
    {
        if (Status == HotelStatus.Closed)
        {
            throw new DomainException("Closed hotels cannot be edited.");
        }
    }

    private void Touch(DateTimeOffset updatedAt)
    {
        UpdatedAt = updatedAt;
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
