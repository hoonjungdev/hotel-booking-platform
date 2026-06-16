using HotelBooking.Api;

namespace HotelBooking.IntegrationTests;

public class ScaffoldingTests
{
    [Fact]
    public void Api_entry_point_is_available_for_future_integration_tests()
    {
        Assert.NotNull(typeof(ApiAssembly).Assembly);
    }
}
