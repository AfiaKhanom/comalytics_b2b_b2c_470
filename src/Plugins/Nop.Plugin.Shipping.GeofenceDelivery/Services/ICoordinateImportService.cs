using Nop.Plugin.Shipping.GeofenceDelivery.Models;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Services;

/// <summary>
/// Represents the coordinate import service interface
/// </summary>
public interface ICoordinateImportService
{
    Task<IList<CoordinateDto>> ParseCoordinatesFromJsonAsync(string json);
    Task<IList<CoordinateDto>> ParseCoordinatesFromCsvAsync(string csv);
    string SerializeCoordinatesAsync(IList<CoordinateDto> coordinates);
}
