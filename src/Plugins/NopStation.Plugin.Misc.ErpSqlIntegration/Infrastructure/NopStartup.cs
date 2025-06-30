using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;
using NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Factories;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services.SqLQueryTemplates;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Infrastructure;

/// <summary>
/// Represents object for the configuring services on application startup
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        //add view location expander
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });

        services.AddScoped<ISqlIntegrationService, SqlIntegrationService>();
        services.AddScoped<IErpNopMapperService, ErpNopMapperService>();
        services.AddScoped<IB2BAccountService, B2BAccountService>();
        services.AddScoped<IB2BProductService, B2BProductService>();
        services.AddScoped<IB2BPricingService, B2BPricingService>();
        services.AddScoped<IB2BInvoiceService, B2BInvoiceService>(); 
        services.AddScoped<IB2BStockService, B2BStockService>();
        services.AddScoped<IShipToAddressService, ShipToAddressService>();
        services.AddScoped<IErpOrderService, ErpOrderService>();
        services.AddScoped<ISqlQueryTemplateModelFactory, SqlQueryTemplateModelFactory>();
        services.AddScoped<ISqlQueryTemplatService, SqlQueryTemplatService>();

        services.AddHttpClient<SqlClient>().WithProxy(); 
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 3000;
}