var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

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

public partial class Program;
