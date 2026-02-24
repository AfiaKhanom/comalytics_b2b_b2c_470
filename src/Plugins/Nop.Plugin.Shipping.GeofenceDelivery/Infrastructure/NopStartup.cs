using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Shipping.GeofenceDelivery.Services;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IGeofenceZoneService, GeofenceZoneService>();
        services.AddScoped<IGeofenceValidationService, GeofenceValidationService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
