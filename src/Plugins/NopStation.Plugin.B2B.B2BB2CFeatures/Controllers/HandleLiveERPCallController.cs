using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Payments.B2BCustomerAccount.Controllers;

public class HandleLiveErpCallController : BasePublicController
{
    #region Fields

    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IErpAccountService _erpAccountService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpLogsService _erpLogsService;
    private readonly ICategoryService _categoryService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpPriceSyncFunctionalityService _erpPriceSyncFunctionalityService;

    #endregion

    #region Ctor

    public HandleLiveErpCallController(IGenericAttributeService genericAttributeService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        B2BB2CFeaturesSettings b2BCustomerAccountSettings,
        IDateTimeHelper dateTimeHelper,
        IErpAccountService erpAccountService,
        ILocalizationService localizationService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpSalesOrgService erpSalesOrgService,
        IErpLogsService erpLogsService,
        ICategoryService categoryService,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpPriceSyncFunctionalityService erpPriceSyncFunctionalityService)
    {
        _genericAttributeService = genericAttributeService;
        _storeContext = storeContext;
        _workContext = workContext;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _b2BB2CFeaturesSettings = b2BCustomerAccountSettings;
        _dateTimeHelper = dateTimeHelper;
        _erpAccountService = erpAccountService;
        _localizationService = localizationService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpSalesOrgService = erpSalesOrgService;
        _erpLogsService = erpLogsService;
        _categoryService = categoryService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpPriceSyncFunctionalityService = erpPriceSyncFunctionalityService;
    }

    #endregion

    #region Utilities

    private async Task<ErpResponseData<ErpPriceSpecialPricingDataModel>> CallErpIntegrationPluginToLivePriceCheck(ErpAccount b2BAccount, ErpSalesOrg accountSalesOrg, Product product)
    {
        try
        {
            var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
            var result = await erpIntegrationPlugin.GetProductSpecialPriceFromErpAsync(new ErpGetRequestModel
            {
                Location = accountSalesOrg.Code,
                AccountNumber = b2BAccount.AccountNumber,
                ProductSku = product.Sku,
            });
            return result;
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.SpecialPrice,
                    $"ERP Integration Product List Live Price Sync: Request failed, Error Occured for "
                    + $"Account Number: {b2BAccount.AccountNumber} and Id: {b2BAccount.Id} and Product sku: {product.Sku}",
                    ex.StackTrace
                );
            return null;
        }
    }

    private async Task<string> ProductListLivePriceSync(ErpAccount b2BAccount, IList<Product> products)
    {
        if (b2BAccount == null || products == null || !products.Any())
            return string.Empty;

        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
        if (erpIntegrationPlugin is null)
        {
            await _erpLogsService.InformationAsync(
                   $"Live price check failed. No Erp Integration Plugin is found.",
                   ErpSyncLevel.SpecialPrice);
            return string.Empty;
        }

        var accountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(b2BAccount.ErpSalesOrgId);
        var priceChangedProductsSkus = string.Empty;

        if (accountSalesOrg != null && !string.IsNullOrEmpty(accountSalesOrg.Code))
        {
            var mpn_skus = products
                .Where(x => !string.IsNullOrEmpty(x.ManufacturerPartNumber))
                .Select(x => $"{x.ManufacturerPartNumber}|{x.Sku}")
                .ToList();

            var skucommaSeparatedString = string.Join(',', mpn_skus);

            if (string.IsNullOrEmpty(skucommaSeparatedString))
                return string.Empty;

            try
            {
                var erpResponseData = new List<ErpPriceSpecialPricingDataModel>();

                foreach (var product in products)
                {
                    var result = await CallErpIntegrationPluginToLivePriceCheck(b2BAccount, accountSalesOrg, product);

                    if (
                       result == null
                       || result.ErpResponseModel == null
                       || result.Data == null
                        || result.ErpResponseModel.IsError
                       )
                    {
                        continue;
                    }
                    else
                    {
                        erpResponseData.Add(result.Data);
                    }

                    var json = JsonConvert.SerializeObject(result, Formatting.Indented);
                    await _erpLogsService.InformationAsync(
                       $"LIVE PIRCE SYNC: Account Number: {b2BAccount.AccountNumber}, " +
                       $"Account Id: {b2BAccount.Id}, " +
                       $"Product sku: {product.Sku}, \n" +
                       $"Response Data: {json}",
                       ErpSyncLevel.SpecialPrice);
                }

                if (erpResponseData.Any())
                {
                    var priceChangedProductsList = new List<string>();

                    foreach (var product in products)
                    {
                        var productResponseModel = erpResponseData
                            .FirstOrDefault(x => x.Sku == product.Sku);
                        if (productResponseModel != null)
                        {
                            var categoryIds = _b2BB2CFeaturesSettings.SkipLivePriceCheckCategoryIds.Split(',').Select(int.Parse).ToList();
                            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);
                            var skipProduct = false;
                            foreach (var cat in productCategories)
                            {
                                if (categoryIds.Any(a => a == cat.CategoryId))
                                    skipProduct = true;
                            }
                            if (skipProduct)
                                continue;

                            var accountPrice = productResponseModel.SpecialPrice;
                            if (accountPrice.HasValue)
                            {
                                var productPricing = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(b2BAccount.Id, product.Id);


                                if (productPricing != null)
                                {
                                    if (accountPrice.Value != productPricing.Price)
                                    {
                                        productPricing.Price = accountPrice.Value;
                                        productPricing.PricingNote = productResponseModel.PricingNotes;

                                        //cr7676
                                        productPricing.DiscountPerc =
                                            productResponseModel.DiscountPercentage.HasValue
                                                ? productResponseModel.DiscountPercentage.Value
                                                : 0;

                                        //productPricing.CustomerUoM = productResponseModel.UnitofMeasure;
                                        await _erpSpecialPriceService.UpdateErpSpecialPriceAsync(productPricing);

                                        priceChangedProductsList.Add(productResponseModel.Sku);
                                    }
                                }
                            }
                        }
                    }

                    if (priceChangedProductsList.Count > 0)
                    {
                        priceChangedProductsSkus = string.Join(',', priceChangedProductsList);
                    }
                }
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.SpecialPrice,
                    $"ERP Integration Product List Live Price Sync: Request failed, Error Occured for "
                    + $"Account Number: {b2BAccount.AccountNumber} and Id: {b2BAccount.Id} and Product skus: {skucommaSeparatedString}",
                    ex.StackTrace
                );
            }
        }

        return priceChangedProductsSkus;
    }

    #endregion

    #region Methods

    protected async Task<string> UpdateCartItemProductLivePrice(ErpAccount erpAccount)
    {
        if (erpAccount is null)
            return string.Empty;

        var currStore = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, currStore.Id);

        if (!cart.Any())
            return string.Empty;

        var productIds = cart.Select(x => x.ProductId).ToList();
        var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());

        return await ProductListLivePriceSync(erpAccount, products);
    }

    public async Task<IActionResult> CurrentCartItemsLiveStockCheck()
    {
        if (!_b2BB2CFeaturesSettings.EnableLiveStockChecks)
        {
            return new NullJsonResult();
        }

        var currStore = await _storeContext.GetCurrentStoreAsync();
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);

        if (erpAccount == null)
        {
            return new NullJsonResult();
        }

        var cart = await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.ShoppingCart, currStore.Id);

        if (!cart.Any())
            return new NullJsonResult();

        var productIds = cart.Select(x => x.ProductId).ToList();
        var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());

        var result = await _erpPriceSyncFunctionalityService.ProductListLiveStockSyncAsync(erpAccount, products);

        return Json(new
        {
            success = result.success,
            message = result.message
        });
    }

    public async Task<IActionResult> CurrentCartItemsLivePriceCheck()
    {
        if (!_b2BB2CFeaturesSettings.EnableLivePriceChecks)
        {
            return new NullJsonResult();
        }

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currStore = await _storeContext.GetCurrentStoreAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);

        if (erpAccount == null)
        {
            return new NullJsonResult();
        }

        var updatedPriceProductSkus = string.Empty;

        if (!erpAccount.LastPriceRefresh.HasValue)
        {
            updatedPriceProductSkus = await UpdateCartItemProductLivePrice(erpAccount);
        }
        else
        {
            var priceUpdateOnLocalTime = _dateTimeHelper.ConvertToUtcTime(erpAccount.LastPriceRefresh.Value, DateTimeKind.Utc);

            if (priceUpdateOnLocalTime < DateTime.UtcNow)
            {
                updatedPriceProductSkus = await UpdateCartItemProductLivePrice(erpAccount);
            }
        }

        await _genericAttributeService.SaveAttributeAsync(currCustomer, B2BB2CFeaturesDefaults.CartItemsLivePriceSyncProcessing, false, currStore.Id);

        if (!string.IsNullOrEmpty(updatedPriceProductSkus))
        {
            var msg = string.Format(await _localizationService.GetResourceAsync("Plugins.Payment.B2BCustomerAccount.LivePriceSync.CartItemPriceUpdated"), updatedPriceProductSkus);

            return Json(new
            {
                success = true,
                data = updatedPriceProductSkus,
                message = msg
            });
        }

        return new NullJsonResult();
    }

    #endregion
}