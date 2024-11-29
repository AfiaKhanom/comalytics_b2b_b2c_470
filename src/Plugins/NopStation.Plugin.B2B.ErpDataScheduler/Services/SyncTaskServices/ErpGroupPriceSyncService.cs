using System.Globalization;
using Nop.Services.Catalog;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpGroupPriceSyncService : IErpGroupPriceSyncService
{
    #region Fields

    private readonly IProductService _productService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpGroupPriceService _erpGroupPriceService;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;

    #endregion

    #region Ctor

    public ErpGroupPriceSyncService(
        IProductService productService,
        ISyncLogService erpSyncLogService,
        IErpSalesOrgService erpSalesOrgService,
        IErpGroupPriceService erpGroupPriceService,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService)
    {
        _productService = productService;
        _erpSyncLogService = erpSyncLogService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpGroupPriceService = erpGroupPriceService;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpGroupPriceSyncSuccessfulAsync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                "No integration method found.");

            return false;
        }

        try
        {
            var allErpSalesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();

            if (!allErpSalesOrgs.Any())
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                    ErpSyncLevel.GroupPrice,
                    "No Erp Sales Org found to perform the Erp Group Price Sync service.");

                return false;
            }

            var erpGroupPriceUpdateList = new List<ErpGroupPrice>();
            var erpGroupPriceCodeUpdateList = new List<ErpGroupPriceCode>();
            var erpGroupPriceInsertList = new List<ErpGroupPrice>();
            var erpGroupPriceCodeInsertList = new List<ErpGroupPriceCode>();

            var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                "Erp Group Price Sync started.");

            foreach (var salesOrg in allErpSalesOrgs)
            {
                var start = "0";
                var dateFrom = DateTime.Today;
                var isError = false;
                var totalSyncedSoFar = 0;
                var lastErpGroupPriceCodeSynced = string.Empty;

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        Location = salesOrg.Code,
                    };

                    var response = await erpIntegrationPlugin.GetProductGroupPricesFromErpAsync(erpGetRequestModel);

                    if (!DateTime.TryParseExact(response.ErpResponseModel.Next, "dd-MM-yy hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
                    {
                        dateFrom = DateTime.MinValue.Date;
                    }

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                            ErpSyncLevel.GroupPrice,
                            response.ErpResponseModel.ErrorShortMessage,
                            response.ErpResponseModel.ErrorFullMessage);

                        break;
                    }
                    else if (response.Data is null/* || dateFrom.Date < DateTime.Today.Date*/)
                    {
                        isError = false;
                        break;
                    }

                    start = response.ErpResponseModel.Next;

                    var products = await _productService.GetProductsBySkuAsync(
                        response.Data
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                        .Select(x => x.Sku)
                        .ToArray());

                    if (products is null || products.Count == 0)
                    {
                        isError = false;
                        break;
                    }

                    // GroupPriceCode should be unique
                    // so we'll track these codes to avoid duplication in the erpGroupPriceCodeUpdateList
                    var processedErpGroupPriceCodes = new HashSet<string>();

                    foreach (var erpGroupPrice in response.Data)
                    {
                        var product = products.FirstOrDefault(x => x.Sku == erpGroupPrice.Sku);

                        if (product is null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(erpGroupPrice.GroupPriceCode))
                        {
                            var oldErpGroupPriceCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodedAsync(erpGroupPrice.GroupPriceCode);
                            if (!processedErpGroupPriceCodes.Contains(oldErpGroupPriceCode?.Code ?? string.Empty))
                            {
                                if (oldErpGroupPriceCode == null)
                                {
                                    oldErpGroupPriceCode = new ErpGroupPriceCode();
                                    oldErpGroupPriceCode.Code = erpGroupPrice.GroupPriceCode;
                                    oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                    oldErpGroupPriceCode.CreatedById = 1;
                                    oldErpGroupPriceCode.CreatedOnUtc = DateTime.UtcNow;
                                    oldErpGroupPriceCode.UpdatedById = 1;
                                    oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                    oldErpGroupPriceCode.IsActive = true;
                                    oldErpGroupPriceCode.IsDeleted = false;
                                    //erpGroupPriceCodeInsertList.Add(oldErpGroupPriceCode);
                                    await _erpGroupPriceCodeService.InsertErpGroupPriceCodeAsync(oldErpGroupPriceCode);
                                }
                                else
                                {
                                    oldErpGroupPriceCode.Code = erpGroupPrice.GroupPriceCode;
                                    oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                    oldErpGroupPriceCode.UpdatedById = 1;
                                    oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                    oldErpGroupPriceCode.IsActive = true;
                                    oldErpGroupPriceCode.IsDeleted = false;
                                    //erpGroupPriceCodeUpdateList.Add(oldErpGroupPriceCode);
                                    await _erpGroupPriceCodeService.UpdateErpGroupPriceCodeAsync(oldErpGroupPriceCode);
                                }
                                // mark this code as processed
                                processedErpGroupPriceCodes.Add(oldErpGroupPriceCode.Code);
                            }


                            var oldErpGroupPrice = await _erpGroupPriceService.GetB2BPriceGroupProductPricingByErpPriceGroupCodeAndProductId
                                (productId: product.Id, priceGroupCodeId: oldErpGroupPriceCode.Id);

                            if (oldErpGroupPrice == null)
                            {
                                oldErpGroupPrice = new ErpGroupPrice();
                                oldErpGroupPrice.ErpNopGroupPriceCodeId = oldErpGroupPriceCode.Id;
                                oldErpGroupPrice.NopProductId = product.Id;
                                oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                                oldErpGroupPrice.CreatedById = oldErpGroupPriceCode.CreatedById;
                                oldErpGroupPrice.CreatedOnUtc = DateTime.UtcNow;
                                oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                                oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;
                                oldErpGroupPrice.IsActive = true;
                                oldErpGroupPrice.IsDeleted = false;
                                erpGroupPriceInsertList.Add(oldErpGroupPrice);
                            }
                            else
                            {
                                oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                                oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                                oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;
                                erpGroupPriceUpdateList.Add(oldErpGroupPrice);
                            }

                            totalSyncedSoFar++;
                            lastErpGroupPriceCodeSynced = oldErpGroupPriceCode.Code;
                        }

                        if (erpGroupPrice.GroupPrices.Count != 0)
                        {
                            foreach (var price in erpGroupPrice.GroupPrices)
                            {
                                if (price.Value > 0)
                                {
                                    var oldErpGroupPriceCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodedAsync(price.Key);

                                    if (oldErpGroupPriceCode == null)
                                    {
                                        oldErpGroupPriceCode = new ErpGroupPriceCode();
                                        oldErpGroupPriceCode.Code = price.Key;
                                        oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                        oldErpGroupPriceCode.CreatedById = 1;
                                        oldErpGroupPriceCode.CreatedOnUtc = DateTime.UtcNow;
                                        oldErpGroupPriceCode.UpdatedById = 1;
                                        oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                        oldErpGroupPriceCode.IsActive = true;
                                        oldErpGroupPriceCode.IsDeleted = false;
                                        //erpGroupPriceCodeInsertList.Add(oldErpGroupPriceCode);
                                        await _erpGroupPriceCodeService.InsertErpGroupPriceCodeAsync(oldErpGroupPriceCode);
                                    }
                                    else
                                    {
                                        oldErpGroupPriceCode.Code = price.Key;
                                        oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                        oldErpGroupPriceCode.UpdatedById = 1;
                                        oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                        //erpGroupPriceCodeUpdateList.Add(oldErpGroupPriceCode);
                                        await _erpGroupPriceCodeService.UpdateErpGroupPriceCodeAsync(oldErpGroupPriceCode);
                                    }

                                    var oldErpGroupPrice = await _erpGroupPriceService
                                        .GetB2BPriceGroupProductPricingByErpPriceGroupCodeAndProductId(productId: product.Id, priceGroupCodeId: oldErpGroupPriceCode.Id);
                                    
                                    if (oldErpGroupPrice == null)
                                    {
                                        oldErpGroupPrice = new ErpGroupPrice();
                                        oldErpGroupPrice.ErpNopGroupPriceCodeId = oldErpGroupPriceCode.Id;
                                        oldErpGroupPrice.NopProductId = product.Id;
                                        oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                                        oldErpGroupPrice.CreatedById = oldErpGroupPriceCode.CreatedById;
                                        oldErpGroupPrice.CreatedOnUtc = DateTime.UtcNow;
                                        oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                                        oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;
                                        oldErpGroupPrice.IsActive = true;
                                        oldErpGroupPrice.IsDeleted = false;
                                        erpGroupPriceInsertList.Add(oldErpGroupPrice);
                                    }
                                    else
                                    {
                                        oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                                        oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                                        oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;
                                        erpGroupPriceUpdateList.Add(oldErpGroupPrice);
                                    }

                                    totalSyncedSoFar++;
                                    lastErpGroupPriceCodeSynced = oldErpGroupPriceCode.Code;
                                }
                            }
                        }
                    }

                    processedErpGroupPriceCodes.Clear();

                    if (erpGroupPriceCodeInsertList.Count != 0)
                    {
                        await _erpGroupPriceCodeService.InsertErpGroupPriceCodesAsync(erpGroupPriceCodeInsertList);
                        erpGroupPriceCodeInsertList.Clear();
                    }

                    if (erpGroupPriceCodeUpdateList.Count != 0)
                    {
                        await _erpGroupPriceCodeService.UpdateErpGroupPriceCodesAsync(erpGroupPriceCodeUpdateList);
                        await _erpDataClearCacheService.ClearCacheOfEntities(erpGroupPriceCodeUpdateList);
                        erpGroupPriceCodeUpdateList.Clear();
                    }

                    if (erpGroupPriceInsertList.Count != 0)
                    {
                        await _erpGroupPriceService.InsertErpGroupPricesAsync(erpGroupPriceInsertList);
                        erpGroupPriceInsertList.Clear();
                    }

                    if (erpGroupPriceUpdateList.Count != 0)
                    {
                        await _erpGroupPriceService.UpdateErpGroupPricesAsync(erpGroupPriceUpdateList);
                        await _erpDataClearCacheService.ClearCacheOfEntities(erpGroupPriceUpdateList);
                        erpGroupPriceUpdateList.Clear();
                    }
                }

                if (!isError)
                {
                    await _erpGroupPriceService.InActiveAllOldGroupPrice(syncStartTime);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                        ErpSyncLevel.GroupPrice,
                        $"Erp Group Price sync successful for Sales Org: {salesOrg.Name}. The group prices which were updated before " +
                        $"{syncStartTime} are deactivated.");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                        ErpSyncLevel.GroupPrice,
                        $"Erp Group Price sync is paritally or not successful for Sales Org: {salesOrg.Name}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                        ErpSyncLevel.GroupPrice,
                        (!string.IsNullOrWhiteSpace(lastErpGroupPriceCodeSynced) ?
                        $"The last synced Erp Group Price Code: {lastErpGroupPriceCodeSynced}, for Sales Org: {salesOrg.Name}. " : string.Empty) +
                        $"Total synced in this session: {totalSyncedSoFar}");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                "Erp Group Price Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                ex.Message,
                ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                "Erp Group Price Sync ended.");

            return false;
        }
    }

    #endregion
}