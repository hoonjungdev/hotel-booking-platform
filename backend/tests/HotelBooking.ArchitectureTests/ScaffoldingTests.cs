using HotelBooking.Api;
using HotelBooking.BuildingBlocks;
using HotelBooking.Modules.Booking;
using HotelBooking.Modules.Identity;
using HotelBooking.Modules.Inventory;
using HotelBooking.Modules.Notification;
using HotelBooking.Modules.Payment;
using HotelBooking.Modules.Pricing;
using HotelBooking.Modules.Property;
using HotelBooking.SharedKernel;
using HotelBooking.Worker;

namespace HotelBooking.ArchitectureTests;

public class ScaffoldingTests
{
    [Fact]
    public void Core_project_assemblies_are_loadable()
    {
        var assemblies = new[]
        {
            typeof(ApiAssembly).Assembly,
            typeof(WorkerAssembly).Assembly,
            typeof(SharedKernelAssembly).Assembly,
            typeof(BuildingBlocksAssembly).Assembly,
            typeof(PropertyModuleAssembly).Assembly,
            typeof(InventoryModuleAssembly).Assembly,
            typeof(PricingModuleAssembly).Assembly,
            typeof(BookingModuleAssembly).Assembly,
            typeof(PaymentModuleAssembly).Assembly,
            typeof(NotificationModuleAssembly).Assembly,
            typeof(IdentityModuleAssembly).Assembly
        };

        Assert.All(assemblies, Assert.NotNull);
    }
}
