using System.Text.Json;
using Nop.Plugin.Shipping.GeofenceDelivery.Models;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

/// <summary>
/// Service for importing/parsing coordinates
/// </summary>
public class CoordinateImportService : ICoordinateImportService
{
    public Task<IList<CoordinateDto>> ParseCoordinatesFromJsonAsync(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult<IList<CoordinateDto>>(new List<CoordinateDto>());

        try
        {
            var coordinates = JsonSerializer.Deserialize<List<CoordinateDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Task.FromResult<IList<CoordinateDto>>(coordinates ?? new List<CoordinateDto>());
        }
        catch
        {
            return Task.FromResult<IList<CoordinateDto>>(new List<CoordinateDto>());
        }
    }

    public Task<IList<CoordinateDto>> ParseCoordinatesFromCsvAsync(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return Task.FromResult<IList<CoordinateDto>>(new List<CoordinateDto>());

        var result = new List<CoordinateDto>();
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var seq = 0;

        foreach (var line in lines)
        {
            var parts = line.Trim().Split(',');
            if (parts.Length >= 2 &&
                decimal.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
                decimal.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var lng))
            {
                result.Add(new CoordinateDto { Lat = lat, Lng = lng, Seq = seq++ });
            }
        }

        return Task.FromResult<IList<CoordinateDto>>(result);
    }

    public string SerializeCoordinates(IList<CoordinateDto> coordinates)
    {
        if (coordinates == null || !coordinates.Any())
            return "[]";

        return JsonSerializer.Serialize(coordinates);
    }
}
