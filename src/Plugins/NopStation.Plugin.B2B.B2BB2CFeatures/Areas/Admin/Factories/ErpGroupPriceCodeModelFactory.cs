using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpGroupPriceCodeModelFactory : IErpGroupPriceCodeModelFactory
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IB2BFeaturesCommonHelper _b2BFeaturesCommonHelper;

    #endregion

    #region Ctor

    public ErpGroupPriceCodeModelFactory(
        ILocalizationService localizationService,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
        IErpSalesOrgService erpSalesOrgService,
        IB2BFeaturesCommonHelper b2BFeaturesCommonHelper)
    {
        _localizationService = localizationService;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _erpSalesOrgService = erpSalesOrgService;
        _b2BFeaturesCommonHelper = b2BFeaturesCommonHelper;
    }

    #endregion

    #region Method

    public async Task<ErpGroupPriceCodeListModel> PrepareErpGroupPriceCodeListModelAsync(ErpGroupPriceCodeSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpGroupPriceCodes = await _erpGroupPriceCodeService.GetAllErpGroupPriceCodesPagedAsync(
            searchModel.SearchGroupPriceCode,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2));

        var model = new ErpGroupPriceCodeListModel().PrepareToGrid(searchModel, erpGroupPriceCodes, () =>
        {
            return erpGroupPriceCodes.Select(priceGroup =>
            {
                var priceGroupModel = new ErpGroupPriceCodeModel
                {
                    Id = priceGroup.Id,
                    GroupPriceCode = priceGroup.Code,
                    LastPriceUpdatedOnUTC = priceGroup.LastUpdateTime,
                    IsActive = priceGroup.IsActive,
                };

                return priceGroupModel;
            });
        });
        return model;
    }

    public async Task<ErpGroupPriceCodeModel> PrepareErpGroupPriceCodeModelAsync(ErpGroupPriceCodeModel model, ErpGroupPriceCode erpGroupPriceCode)
    {
        if (erpGroupPriceCode != null)
        {
            model = model ?? new ErpGroupPriceCodeModel();
            model.Id = erpGroupPriceCode.Id;
            model.GroupPriceCode = erpGroupPriceCode.Code;
            model.IsActive = erpGroupPriceCode.IsActive;
            model.LastPriceUpdatedOnUTC = erpGroupPriceCode.LastUpdateTime;
        }
        return model;
    }

    public async Task<ErpGroupPriceCodeSearchModel> PrepareErpGroupPriceCodeSearchModelAsync(ErpGroupPriceCodeSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.ShowInActiveOption = await _b2BFeaturesCommonHelper.PrepareActiveFilterOptionsAsync();

        searchModel.SetGridPageSize();
        return searchModel;
    }

    public async Task PrepareErpGroupPriceCodes(IList<SelectListItem> items, bool withSpecialDefaultItem = false)
    {
        ArgumentNullException.ThrowIfNull(items);

        var availablePriceGroup = await _erpGroupPriceCodeService.GetAllErpGroupPriceCodesAsync();
        foreach (var priceGroup in availablePriceGroup)
        {
            items.Add(new SelectListItem { Value = priceGroup.Id.ToString(), Text = priceGroup.Code });
        }

        if (withSpecialDefaultItem)
            await PrepareDefaultItem(items);
    }

    public async Task PrepareErpSalesOrgs(IList<SelectListItem> availableErpSalesOrgs, bool withSpecialDefaultItem = false)
    {
        ArgumentNullException.ThrowIfNull(availableErpSalesOrgs);

        var availableSalesOrg = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
        foreach (var salesOrg in availableSalesOrg)
        {
            availableErpSalesOrgs.Add(new SelectListItem { Value = salesOrg.Id.ToString(), Text = salesOrg.Name + "_" + salesOrg.Code });
        }

        if (withSpecialDefaultItem)
            await PrepareDefaultItem(availableErpSalesOrgs);
    }

    protected async Task PrepareDefaultItem(IList<SelectListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        const string value = "0";
        var defaultItemText = await _localizationService.GetResourceAsync("Admin.Common.All");
        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });
    }

    #endregion
}
