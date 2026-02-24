using System.Text.Json;
using NetTopologySuite.Geometries;
using Nop.Plugin.Shipping.GeofenceDelivery.Models;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

public class GeofenceValidationService : IGeofenceValidationService
{
    protected readonly IGeofenceZoneService _zoneService;
    protected readonly GeofenceDeliverySettings _settings;

    public GeofenceValidationService(
        IGeofenceZoneService zoneService,
        GeofenceDeliverySettings settings)
    {
        _zoneService = zoneService;
        _settings = settings;
    }

    public async Task<GeofenceValidationResult> ValidateLocationAsync(decimal latitude, decimal longitude)
    {
        var zones = await _zoneService.GetAllZonesAsync(activeOnly: true);
        if (!zones.Any())
            return new GeofenceValidationResult { IsValid = false, Message = _settings.OutsideZoneMessage };

        var point = new GeometryFactory().CreatePoint(new Coordinate((double)longitude, (double)latitude));

        var matchingZones = new List<(Data.Domain.GeofenceZone Zone, bool Inside)>();

        foreach (var zone in zones)
        {
            try
            {
                var coordinates = DeserializeCoordinates(zone.CoordinatesJson);
                if (coordinates.Count < 3)
                    continue;

                var polygon = BuildPolygon(coordinates);
                if (polygon != null && (polygon.Contains(point) || polygon.Boundary.Contains(point) || polygon.Intersects(point)))
                    matchingZones.Add((zone, true));
            }
            catch
            {
                // Skip zones with invalid coordinate data to allow other valid zones to be evaluated
            }
        }

        if (!matchingZones.Any())
            return new GeofenceValidationResult { IsValid = false, Message = _settings.OutsideZoneMessage };

        // Apply lowest fee rule for overlapping zones
        var bestMatch = matchingZones
            .OrderBy(m => m.Zone.DeliveryFee)
            .First();

        return new GeofenceValidationResult
        {
            IsValid = true,
            AssignedZone = bestMatch.Zone,
            IsCollectionOnly = bestMatch.Zone.IsCollectionOnly,
            Message = bestMatch.Zone.IsCollectionOnly
                ? "Collection only available"
                : $"Delivery available - {bestMatch.Zone.Name} (${bestMatch.Zone.DeliveryFee})"
        };
    }

    private static List<CoordinateDto> DeserializeCoordinates(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<CoordinateDto>();
        try
        {
            return JsonSerializer.Deserialize<List<CoordinateDto>>(json) ?? new List<CoordinateDto>();
        }
        catch
        {
            // Return empty list on deserialization failure; invalid JSON coordinates are treated as an empty zone
            return new List<CoordinateDto>();
        }
    }

    private static Polygon BuildPolygon(List<CoordinateDto> coordinates)
    {
        var ordered = coordinates.OrderBy(c => c.Seq).ToList();
        var coords = ordered.Select(c => new Coordinate((double)c.Lng, (double)c.Lat)).ToList();

        // Ensure polygon is closed (first = last)
        if (coords.Count > 0 && (coords[0].X != coords[^1].X || coords[0].Y != coords[^1].Y))
            coords.Add(coords[0]);

        if (coords.Count < 4) // need at least 4 (3 unique + closing point)
            return null;

        var ring = new GeometryFactory().CreateLinearRing(coords.ToArray());
        return new GeometryFactory().CreatePolygon(ring);
    }
}
