using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Factories;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Service.Auth;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Services;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Infrastructure
{
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.B2B.ODataIntegration");

            //add view location expander
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            //register services
            services.AddHttpClient<D365HttpClient>().WithProxy();
            services.AddScoped<ID365Service, D365Service>();
            services.AddScoped<IModelFactory, ModelFactory>();
            services.AddScoped<IIntegrationAuthService, IntegrationAuthService>();

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
}
