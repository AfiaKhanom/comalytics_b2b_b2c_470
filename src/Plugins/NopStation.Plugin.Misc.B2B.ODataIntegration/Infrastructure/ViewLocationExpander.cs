using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";
        private const string ADMIN_AREA = "Admin";

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == ADMIN_AREA)
            {
                viewLocations = new[] {
                    $"/Plugins/NopStation.Plugin.Misc.B2B.ODataIntegration/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.Misc.B2B.ODataIntegration/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);

                return viewLocations;
            }

            viewLocations = new[] {
                $"/Plugins/NopStation.Plugin.Misc.B2B.ODataIntegration/Views/Shared/{{0}}.cshtml",
                $"/Plugins/NopStation.Plugin.Misc.B2B.ODataIntegration/Views/{{1}}/{{0}}.cshtml"
            }.Concat(viewLocations);

            if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                viewLocations = new[] {
                    $"/Plugins/NopStation.Plugin.Misc.B2B.ODataIntegration/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.Misc.B2B.ODataIntegration/Themes/{theme}/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }

            return viewLocations;
        }
    }
}