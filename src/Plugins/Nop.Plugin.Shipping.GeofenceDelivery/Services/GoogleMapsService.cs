using System.Text.Json;
using Nop.Core.Configuration;
using Nop.Plugin.Shipping.GeofenceDelivery.Models;
using Nop.Services.Configuration;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

/// <summary>
/// Google Maps geocoding service
/// </summary>
public class GoogleMapsService : IGoogleMapsService
{
    private const string GeocodeApiUrl = "https://maps.googleapis.com/maps/api/geocode/json";

    private readonly ISettingService _settingService;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleMapsService(ISettingService settingService, IHttpClientFactory httpClientFactory)
    {
        _settingService = settingService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<LocationValidationResult> GeocodeAddressAsync(string address)
    {
        try
        {
            var settings = await _settingService.LoadSettingAsync<GeofenceDeliverySettings>();
            if (string.IsNullOrEmpty(settings.GoogleMapsApiKey))
            {
                return new LocationValidationResult
                {
                    Success = false,
                    ErrorMessage = "Google Maps API key is not configured"
                };
            }

            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"{GeocodeApiUrl}?address={encodedAddress}&key={settings.GoogleMapsApiKey}";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(url);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            var status = root.GetProperty("status").GetString();

            if (status != "OK")
            {
                return new LocationValidationResult
                {
                    Success = false,
                    ErrorMessage = $"Geocoding failed with status: {status}"
                };
            }

            var results = root.GetProperty("results");
            if (results.GetArrayLength() == 0)
            {
                return new LocationValidationResult
                {
                    Success = false,
                    ErrorMessage = "No results found for the address"
                };
            }

            var location = results[0].GetProperty("geometry").GetProperty("location");
            var formattedAddress = results[0].GetProperty("formatted_address").GetString();

            return new LocationValidationResult
            {
                Success = true,
                Latitude = (decimal)location.GetProperty("lat").GetDouble(),
                Longitude = (decimal)location.GetProperty("lng").GetDouble(),
                FormattedAddress = formattedAddress
            };
        }
        catch (Exception ex)
        {
            return new LocationValidationResult
            {
                Success = false,
                ErrorMessage = $"Error during geocoding: {ex.Message}"
            };
        }
    }
}
