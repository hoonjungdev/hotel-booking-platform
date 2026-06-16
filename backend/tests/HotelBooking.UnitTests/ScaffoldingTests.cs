using HotelBooking.SharedKernel;

namespace HotelBooking.UnitTests;

public class ScaffoldingTests
{
    [Fact]
    public void Shared_kernel_assembly_marker_is_available()
    {
        Assert.NotNull(typeof(SharedKernelAssembly).Assembly);
    }
}
