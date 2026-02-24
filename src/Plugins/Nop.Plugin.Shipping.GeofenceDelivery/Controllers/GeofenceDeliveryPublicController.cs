using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Controllers;

public class GeofenceDeliveryPublicController : BasePluginController
{
    protected readonly IGeofenceValidationService _validationService;

    public GeofenceDeliveryPublicController(IGeofenceValidationService validationService)
    {
        _validationService = validationService;
    }

    [HttpGet]
    public async Task<IActionResult> ValidateLocation(decimal latitude, decimal longitude)
    {
        var result = await _validationService.ValidateLocationAsync(latitude, longitude);
        return Json(new
        {
            isValid = result.IsValid,
            message = result.Message,
            isCollectionOnly = result.IsCollectionOnly,
            zoneName = result.AssignedZone?.Name,
            deliveryFee = result.AssignedZone?.DeliveryFee
        });
    }
}
