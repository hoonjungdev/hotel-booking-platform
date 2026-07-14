using System.Net;
using System.Net.Http.Json;
using HotelBooking.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelBooking.IntegrationTests;

public sealed class ApiStatusEndpointTests : IClassFixture<WebApplicationFactory<ApiAssembly>>
{
    private readonly HttpClient _client;

    public ApiStatusEndpointTests(WebApplicationFactory<ApiAssembly> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_root_returns_the_ready_api_status_contract()
    {
        HttpResponseMessage response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiStatusResponse? body = await response.Content.ReadFromJsonAsync<ApiStatusResponse>();

        Assert.NotNull(body);
        Assert.Equal("Hotel Booking API", body.Name);
        Assert.Equal("ready", body.Status);
    }

    private sealed record ApiStatusResponse(string Name, string Status);
}
