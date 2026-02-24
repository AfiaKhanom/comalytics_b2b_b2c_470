using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.GeofenceDelivery.Models;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Controllers;

/// <summary>
/// Public controller for geofence delivery operations
/// </summary>
public class GeofenceDeliveryPublicController : Controller
{
    private readonly IGeofenceValidationService _geofenceValidationService;

    public GeofenceDeliveryPublicController(IGeofenceValidationService geofenceValidationService)
    {
        _geofenceValidationService = geofenceValidationService;
    }

    [HttpPost]
    public async Task<IActionResult> ValidateAddress([FromBody] AddressValidationRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = await _geofenceValidationService.ValidateAddressAsync(request);
        return Json(result);
    }
}
