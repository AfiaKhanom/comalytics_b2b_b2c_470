using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: "Plugin.Shipping.GeofenceDelivery.ValidateAddress",
            pattern: "geofence/validate-address",
            defaults: new { controller = "GeofenceDeliveryPublic", action = "ValidateAddress" });
    }

    public int Priority => 0;
}
