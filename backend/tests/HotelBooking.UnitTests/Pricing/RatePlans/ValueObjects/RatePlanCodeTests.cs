using HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Pricing.RatePlans.ValueObjects;

/// <summary>
/// Proves canonical formatting and required input rules for hotel-defined RatePlan codes.
/// </summary>
public class RatePlanCodeTests
{
    [Fact]
    public void Create_trims_and_normalizes_code_case()
    {
        RatePlanCode code = RatePlanCode.Create("  flex-bb  ");

        Assert.Equal("FLEX-BB", code.Value);
        Assert.Equal("FLEX-BB", code.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_missing_code(string? code)
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            RatePlanCode.Create(code!));

        Assert.Equal("code", exception.ParamName);
    }

    [Fact]
    public void Equivalent_codes_are_equal_regardless_of_input_case()
    {
        RatePlanCode lowerCase = RatePlanCode.Create("flex-bb");
        RatePlanCode upperCase = RatePlanCode.Create("FLEX-BB");

        Assert.Equal(lowerCase, upperCase);
    }
}
