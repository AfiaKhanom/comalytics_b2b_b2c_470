using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Shipping.GeofenceDelivery.Areas.Admin.Factories;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure;

/// <summary>
/// Represents object for the configuring services on application startup
/// </summary>
public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IGeofenceZoneService, GeofenceZoneService>();
        services.AddScoped<IGeofenceValidationService, GeofenceValidationService>();
        services.AddScoped<IGoogleMapsService, GoogleMapsService>();
        services.AddScoped<ICoordinateImportService, CoordinateImportService>();
        services.AddScoped<IGeofenceModelFactory, GeofenceModelFactory>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
