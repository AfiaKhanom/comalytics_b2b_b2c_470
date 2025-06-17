using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services;
using Nop.Services.Localization;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;

public class B2BFeaturesCommonHelper : IB2BFeaturesCommonHelper
{
    private readonly ILocalizationService _localizationService;

    public B2BFeaturesCommonHelper(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items, 
        bool withSpecialDefaultItem = true, 
        string defaultItemText = null, 
        string defaultItemValue = "0", 
        bool selected = false)
    {
        ArgumentNullException.ThrowIfNull(items);

        //whether to insert the first special item for the default value
        if (!withSpecialDefaultItem)
            return;

        //prepare item text
        defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.Select");

        //insert this default item at first
        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue, Selected = selected });
    }

    public async Task<List<SelectListItem>> PrepareDropdownDataFromEnumAsync<TEnum>(bool withSpecialDefaultItem = true, 
        string defaultItemText = null, 
        string defaultItemValue = "0", 
        bool selected = false) where TEnum : Enum
    {
        var items = Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(e => new SelectListItem
            {
                Value = $"{Convert.ToInt32(e)}",
                Text = $"{e}"
            }).ToList();

        await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText, defaultItemValue, selected);

        return items;
    }

    public async Task<List<SelectListItem>> PrepareActiveFilterOptionsAsync()
    {
        var options = new List<SelectListItem>();

        var availableActiveFilterTypes = await ActiveFilterType.ShowAll.ToSelectListAsync(false);
        foreach (var filterType in availableActiveFilterTypes)
        {
            var enumValue = Enum.Parse(typeof(ActiveFilterType), filterType.Value);
            var resourceKey = $"NopStation.Plugin.B2B.ERPIntegrationCore.Common.ActiveFilter.{enumValue}";
            filterType.Text = await _localizationService.GetResourceAsync(resourceKey);
            options.Add(filterType);
        }

        return options;
    }
}