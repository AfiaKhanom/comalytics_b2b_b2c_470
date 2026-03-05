using NetTopologySuite.Geometries;
using Nop.Plugin.Shipping.GeofenceDelivery.Models;
using System.Text.Json;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

/// <summary>
/// Geofence validation service using NetTopologySuite for point-in-polygon calculations
/// </summary>
public class GeofenceValidationService : IGeofenceValidationService
{
    private readonly IGeofenceZoneService _geofenceZoneService;
    private readonly IGoogleMapsService _googleMapsService;

    public GeofenceValidationService(IGeofenceZoneService geofenceZoneService,
        IGoogleMapsService googleMapsService)
    {
        _geofenceZoneService = geofenceZoneService;
        _googleMapsService = googleMapsService;
    }

    public async Task<GeofenceValidationResult> ValidateLocationAsync(decimal latitude, decimal longitude)
    {
        var zones = await _geofenceZoneService.GetActiveZonesAsync();

        foreach (var zone in zones)
        {
            if (string.IsNullOrEmpty(zone.CoordinatesJson))
                continue;

            var coordinates = JsonSerializer.Deserialize<List<CoordinateDto>>(zone.CoordinatesJson);
            if (coordinates == null || coordinates.Count < 3)
                continue;

            if (IsPointInZone(latitude, longitude, coordinates))
            {
                return new GeofenceValidationResult
                {
                    IsValid = true,
                    ZoneId = zone.Id,
                    ZoneName = zone.Name,
                    DeliveryFee = zone.DeliveryFee,
                    IsCollectionOnly = zone.IsCollectionOnly
                };
            }
        }

        return new GeofenceValidationResult
        {
            IsValid = false,
            ErrorMessage = "Address is outside all delivery zones"
        };
    }

    public async Task<GeofenceValidationResult> ValidateAddressAsync(AddressValidationRequest request)
    {
        decimal latitude;
        decimal longitude;

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            latitude = request.Latitude.Value;
            longitude = request.Longitude.Value;
        }
        else
        {
            var addressString = $"{request.Address1}, {request.City}, {request.ZipPostalCode}, {request.Country}";
            var locationResult = await _googleMapsService.GeocodeAddressAsync(addressString);

            if (!locationResult.Success)
            {
                return new GeofenceValidationResult
                {
                    IsValid = false,
                    ErrorMessage = locationResult.ErrorMessage ?? "Failed to geocode address"
                };
            }

            latitude = locationResult.Latitude;
            longitude = locationResult.Longitude;
        }

        return await ValidateLocationAsync(latitude, longitude);
    }

    public bool IsPointInZone(decimal latitude, decimal longitude, IList<CoordinateDto> coordinates)
    {
        if (coordinates == null || coordinates.Count < 3)
            return false;

        var factory = new GeometryFactory();
        var orderedCoords = coordinates.OrderBy(c => c.Seq).ToList();

        // Build the polygon ring - must close the polygon
        var ringCoords = orderedCoords
            .Select(c => new Coordinate((double)c.Lng, (double)c.Lat))
            .ToList();

        // Close the polygon if not already closed
        if (!ringCoords[0].Equals2D(ringCoords[ringCoords.Count - 1]))
            ringCoords.Add(ringCoords[0]);

        if (ringCoords.Count < 4)
            return false;

        var ring = factory.CreateLinearRing(ringCoords.ToArray());
        var polygon = factory.CreatePolygon(ring);
        var point = factory.CreatePoint(new Coordinate((double)longitude, (double)latitude));

        return polygon.Contains(point) || polygon.Touches(point);
    }
}
