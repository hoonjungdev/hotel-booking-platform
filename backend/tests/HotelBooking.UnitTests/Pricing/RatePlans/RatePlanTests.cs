using HotelBooking.Modules.Pricing.Domain.RatePlans;
using HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;
using HotelBooking.Modules.Pricing.Domain.References;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.Pricing.RatePlans;

/// <summary>
/// Proves RatePlan creation invariants and lifecycle transitions.
/// </summary>
public class RatePlanTests
{
    private static readonly RatePlanId ExistingRatePlanId =
        RatePlanId.From(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly HotelId ExistingHotelId =
        HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static readonly RoomTypeId ExistingRoomTypeId =
        RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000001"));

    private static readonly Currency SellingCurrency = Currency.FromCode("KRW");

    private static readonly HotelRateSettings ExistingHotelRateSettings =
        HotelRateSettings.Create(ExistingHotelId, SellingCurrency);

    [Fact]
    public void CreateDraft_creates_rate_plan_in_draft_status()
    {
        RatePlanCode code = RatePlanCode.Create("flex-bb");
        CancellationPolicy cancellationPolicy = CreateCancellationPolicy();

        RatePlan ratePlan = RatePlan.CreateDraft(
            ExistingRatePlanId,
            ExistingHotelRateSettings,
            ExistingRoomTypeId,
            "  Flexible Breakfast Included  ",
            code,
            cancellationPolicy);

        Assert.Equal(ExistingRatePlanId, ratePlan.Id);
        Assert.Equal(ExistingHotelRateSettings, ratePlan.HotelRateSettings);
        Assert.Equal(ExistingHotelId, ratePlan.HotelId);
        Assert.Equal(ExistingRoomTypeId, ratePlan.RoomTypeId);
        Assert.Equal(SellingCurrency, ratePlan.SellingCurrency);
        Assert.Equal("Flexible Breakfast Included", ratePlan.Name);
        Assert.Equal(code, ratePlan.Code);
        Assert.Equal(cancellationPolicy, ratePlan.CancellationPolicy);
        Assert.Equal(RatePlanStatus.Draft, ratePlan.Status);
    }

    [Fact]
    public void CreateDraft_rejects_default_rate_plan_id()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CreateDraftRatePlan(id: new RatePlanId()));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void CreateDraft_rejects_missing_hotel_rate_settings()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            RatePlan.CreateDraft(
                ExistingRatePlanId,
                null!,
                ExistingRoomTypeId,
                "Flexible Breakfast Included",
                RatePlanCode.Create("FLEX-BB"),
                CreateCancellationPolicy()));

        Assert.Equal("hotelRateSettings", exception.ParamName);
    }

    [Fact]
    public void CreateDraft_rejects_default_room_type_id()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CreateDraftRatePlan(roomTypeId: new RoomTypeId()));

        Assert.Equal("roomTypeId", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateDraft_rejects_missing_name(string? name)
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CreateDraftRatePlan(name: name!));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void CreateDraft_rejects_missing_code()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            RatePlan.CreateDraft(
                ExistingRatePlanId,
                ExistingHotelRateSettings,
                ExistingRoomTypeId,
                "Flexible Breakfast Included",
                null!,
                CreateCancellationPolicy()));

        Assert.Equal("code", exception.ParamName);
    }

    [Fact]
    public void CreateDraft_rejects_missing_cancellation_policy()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            RatePlan.CreateDraft(
                ExistingRatePlanId,
                ExistingHotelRateSettings,
                ExistingRoomTypeId,
                "Flexible Breakfast Included",
                RatePlanCode.Create("FLEX-BB"),
                null!));

        Assert.Equal("cancellationPolicy", exception.ParamName);
    }

    [Fact]
    public void Activate_moves_draft_rate_plan_to_active()
    {
        RatePlan ratePlan = CreateDraftRatePlan();

        ratePlan.Activate();

        Assert.Equal(RatePlanStatus.Active, ratePlan.Status);
    }

    [Theory]
    [InlineData(RatePlanStatus.Active)]
    [InlineData(RatePlanStatus.Suspended)]
    [InlineData(RatePlanStatus.Closed)]
    public void Activate_rejects_non_draft_rate_plan(RatePlanStatus status)
    {
        RatePlan ratePlan = CreateRatePlanWithStatus(status);

        Assert.Throws<DomainException>(ratePlan.Activate);
        Assert.Equal(status, ratePlan.Status);
    }

    [Fact]
    public void Suspend_moves_active_rate_plan_to_suspended()
    {
        RatePlan ratePlan = CreateRatePlanWithStatus(RatePlanStatus.Active);

        ratePlan.Suspend();

        Assert.Equal(RatePlanStatus.Suspended, ratePlan.Status);
    }

    [Theory]
    [InlineData(RatePlanStatus.Draft)]
    [InlineData(RatePlanStatus.Suspended)]
    [InlineData(RatePlanStatus.Closed)]
    public void Suspend_rejects_non_active_rate_plan(RatePlanStatus status)
    {
        RatePlan ratePlan = CreateRatePlanWithStatus(status);

        Assert.Throws<DomainException>(ratePlan.Suspend);
        Assert.Equal(status, ratePlan.Status);
    }

    [Fact]
    public void Reactivate_moves_suspended_rate_plan_to_active()
    {
        RatePlan ratePlan = CreateRatePlanWithStatus(RatePlanStatus.Suspended);

        ratePlan.Reactivate();

        Assert.Equal(RatePlanStatus.Active, ratePlan.Status);
    }

    [Theory]
    [InlineData(RatePlanStatus.Draft)]
    [InlineData(RatePlanStatus.Active)]
    [InlineData(RatePlanStatus.Closed)]
    public void Reactivate_rejects_non_suspended_rate_plan(RatePlanStatus status)
    {
        RatePlan ratePlan = CreateRatePlanWithStatus(status);

        Assert.Throws<DomainException>(ratePlan.Reactivate);
        Assert.Equal(status, ratePlan.Status);
    }

    [Fact]
    public void Close_permanently_closes_suspended_rate_plan()
    {
        RatePlan ratePlan = CreateRatePlanWithStatus(RatePlanStatus.Suspended);

        ratePlan.Close();

        Assert.Equal(RatePlanStatus.Closed, ratePlan.Status);
    }

    [Fact]
    public void Close_is_idempotent_for_an_already_closed_rate_plan()
    {
        RatePlan ratePlan = CreateRatePlanWithStatus(RatePlanStatus.Closed);

        ratePlan.Close();

        Assert.Equal(RatePlanStatus.Closed, ratePlan.Status);
    }

    [Theory]
    [InlineData(RatePlanStatus.Draft)]
    [InlineData(RatePlanStatus.Active)]
    public void Close_rejects_rate_plan_that_has_not_been_suspended(RatePlanStatus status)
    {
        RatePlan ratePlan = CreateRatePlanWithStatus(status);

        Assert.Throws<DomainException>(ratePlan.Close);
        Assert.Equal(status, ratePlan.Status);
    }

    private static RatePlan CreateRatePlanWithStatus(RatePlanStatus status)
    {
        RatePlan ratePlan = CreateDraftRatePlan();

        switch (status)
        {
            case RatePlanStatus.Draft:
                return ratePlan;
            case RatePlanStatus.Active:
                ratePlan.Activate();
                return ratePlan;
            case RatePlanStatus.Suspended:
                ratePlan.Activate();
                ratePlan.Suspend();
                return ratePlan;
            case RatePlanStatus.Closed:
                ratePlan.Activate();
                ratePlan.Suspend();
                ratePlan.Close();
                return ratePlan;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    private static RatePlan CreateDraftRatePlan(
        RatePlanId? id = null,
        RoomTypeId? roomTypeId = null,
        string name = "Flexible Breakfast Included",
        RatePlanCode? code = null,
        CancellationPolicy? cancellationPolicy = null)
    {
        return RatePlan.CreateDraft(
            id ?? ExistingRatePlanId,
            ExistingHotelRateSettings,
            roomTypeId ?? ExistingRoomTypeId,
            name,
            code ?? RatePlanCode.Create("FLEX-BB"),
            cancellationPolicy ?? CreateCancellationPolicy());
    }

    private static CancellationPolicy CreateCancellationPolicy()
    {
        return CancellationPolicy.Create(
            new CancellationRule(TimeSpan.Zero, CancellationPenalty.NoPenalty()));
    }
}
