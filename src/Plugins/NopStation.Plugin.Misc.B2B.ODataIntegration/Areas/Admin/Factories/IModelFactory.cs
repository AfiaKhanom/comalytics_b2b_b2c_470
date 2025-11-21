using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Configuration;
using Nop.Web.Framework.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Factories;
public interface IModelFactory
{
    List<SelectListItem> GetPropertiesToExcludeSelectList<TSetting, TModel>(TSetting settings, TModel model)
    where TSetting : class, ISettings
    where TModel : class, ISettingsModel;
    Task<List<string>> GetPropertiesToExcludeSelectedItemsAsync(Func<ERPIntegrationCoreDataMappingSettings, string?> propertySelector);

}
