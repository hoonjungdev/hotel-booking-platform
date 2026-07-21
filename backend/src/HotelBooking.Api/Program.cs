using HotelBooking.Modules.Inventory;
using HotelBooking.Modules.Property;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

string hotelBookingConnectionString =
    builder.Configuration.GetConnectionString("hotelbooking") ??
    throw new InvalidOperationException("The hotelbooking PostgreSQL connection string is required.");

builder.Services.AddOpenApi();
builder.Services.AddInventoryModule(hotelBookingConnectionString);
builder.Services.AddPropertyModule(hotelBookingConnectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    name = "Hotel Booking API",
    status = "ready"
}))
.WithName("GetApiStatus");

app.Run();

/// <summary>
/// Exposes the generated API entry point to integration tests.
/// </summary>
public partial class Program;
