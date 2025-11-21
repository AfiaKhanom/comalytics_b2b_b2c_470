using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.B2B.ERPIntegrationCore;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Factories;
public class ModelFactory : IModelFactory
{
    #region Fields
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public ModelFactory(
        ISettingService settingService, 
        IStoreContext storeContext)
    {
        _settingService = settingService;
        _storeContext = storeContext;
    }


    #endregion

    #region Methods

    public List<SelectListItem> GetPropertiesToExcludeSelectList<TSetting, TModel>(TSetting settings, TModel model)
    where TSetting : class, ISettings
    where TModel : class, ISettingsModel
    {
        var selectList = new List<SelectListItem>();

        var modelType = typeof(TModel);
        var settingType = typeof(TSetting);

        var modelProperties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var settingProperties = settingType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var settingPropertyNames = new HashSet<string>(settingProperties.Select(p => p.Name));

        foreach (var prop in modelProperties)
        {
            if (prop.PropertyType != typeof(string))
                continue;

            if (!settingPropertyNames.Contains(prop.Name))
                continue;

            var resourceAttr = prop.GetCustomAttribute<NopResourceDisplayNameAttribute>();
            if (resourceAttr == null)
                continue;

            selectList.Add(new SelectListItem
            {
                Text = resourceAttr.DisplayName,
                Value = prop.Name
            });
        }

        return selectList;
    }

    public async Task<List<string>> GetPropertiesToExcludeSelectedItemsAsync(Func<ERPIntegrationCoreDataMappingSettings, string?> propertySelector)
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<ERPIntegrationCoreDataMappingSettings>(storeScope);

        var rawValue = propertySelector(settings);

        var result = rawValue?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList() ?? new List<string>();

        return result;
    }
    #endregion
}