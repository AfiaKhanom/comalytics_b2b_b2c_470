using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpStockSyncService : IErpStockSyncService
{
    #region Fields

    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpProductService _erpProductService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;
    private readonly ERPIntegrationCoreDataMappingSettings _dataMappingSettings;

    #endregion

    #region Ctor

    public ErpStockSyncService(ISyncLogService erpSyncLogService,
        IErpProductService erpProductService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IErpSalesOrgService erpSalesOrgService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IStaticCacheManager staticCacheManager,
        ISyncWorkflowMessageService syncWorkflowMessageService,
        ERPIntegrationCoreDataMappingSettings dataMappingSettings)
    {
        _erpSyncLogService = erpSyncLogService;
        _erpProductService = erpProductService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _staticCacheManager = staticCacheManager;
        _syncWorkflowMessageService = syncWorkflowMessageService;
        _dataMappingSettings = dataMappingSettings;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpStockSyncSuccessfulAsync(string? stockCode, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default)
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                $"No integration method found. Unable to run {ErpDataSchedulerDefaults.ErpStockSyncTaskName}.");

            return false;
        }

        try
        {
            #region Data collection

            var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
            if (!salesOrgs.Any())
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                    ErpSyncLevel.Stock,
                    $"No Sales org found. Unable to run {ErpDataSchedulerDefaults.ErpStockSyncTaskName}.");

                return false;
            }

            var propsToSkip = new HashSet<string>(
                _dataMappingSettings.StockPropertiesToExclude?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    ?? Enumerable.Empty<string>()
            );

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                "Erp Stock Sync started.");

            foreach (var salesOrg in salesOrgs)
            {
                var erpSalesOrgWarehouseMaps = await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapsBySalesOrgIdAsync(salesOrg.Id);
                if (erpSalesOrgWarehouseMaps == null || erpSalesOrgWarehouseMaps.Count == 0)
                    continue;
                var mapsByCode = erpSalesOrgWarehouseMaps
                    .Where(m => !string.IsNullOrWhiteSpace(m.WarehouseCode))
                    .GroupBy(m => m.WarehouseCode.Trim().ToLower())
                    .ToDictionary(g => g.Key, g => g.Last());

                foreach (var salesOrgWarehouseMap in mapsByCode.Values)
                {
                    var start = "0";
                    var isError = false;
                    var lastErpProductStockSynced = string.Empty;
                    var totalSyncedSoFar = 0;
                    var totalNotSyncedSoFar = 0;

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            LastChangedDate = isIncrementalSync ? salesOrg.LastErpStockSyncTimeOnUtc : null,
                            WarehouseCode = salesOrgWarehouseMap.WarehouseCode,
                            ProductSku = stockCode
                        };

                        var response = await erpIntegrationPlugin.GetStocksFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError)
                        {
                            isError = true;

                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                                ErpSyncLevel.Stock,
                                response.ErpResponseModel?.ErrorShortMessage ?? string.Empty,
                                response.ErpResponseModel?.ErrorFullMessage ?? string.Empty);

                            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                                DateTime.UtcNow,
                                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                                response.ErpResponseModel?.ErrorShortMessage + "\n\n" + response.ErpResponseModel?.ErrorFullMessage);

                            break;
                        }
                        else if (response.Data is null || !response.Data.Any())
                        {
                            isError = false;
                            break;
                        }

                        start = response.ErpResponseModel.Next;

                        var pwiToInsert = new List<ProductWarehouseInventory>();
                        var pwiToUpdate = new List<ProductWarehouseInventory>();
                        var stockQuantityHistoriesToInsert = new List<StockQuantityHistory>();

                    var responseData = response.Data
                            .Where(x => !string.IsNullOrWhiteSpace(x.Sku.Trim().ToLower()) &&
                                                            !string.IsNullOrWhiteSpace(x.WarehouseNameOrCode) &&
                                                            x.WarehouseNameOrCode == salesOrgWarehouseMap.WarehouseCode)
                            .GroupBy(x => new { Sku = x.Sku.Trim().ToLower(), WarehouseCode = x.WarehouseNameOrCode })
                            .Select(g => g.Last());

                        totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                    var products = await _erpProductService
                        .GetProductsBySkuAsync(
                            responseData
                            .Select(x => x.Sku?.Trim()?.ToLower())?.ToArray(),
                            filterOutDeleted: true
                        );

                        if (products is null || products.Count == 0)
                        {
                            totalNotSyncedSoFar += response.Data.Count;
                            isError = false;
                            continue;
                        }

                        var inventories = await _erpProductService.GetProductWarehouseInventoryByProductIdsAndNopWarehouseIdsAsync(products.Select(x => x.Id).ToArray(), salesOrgWarehouseMap?.NopWarehouseId ?? 0);

                        foreach (var erpStock in responseData)
                        {
                            if (string.IsNullOrWhiteSpace(erpStock.Sku) || !erpStock.QuantityOnHand.HasValue)
                            {
                                totalNotSyncedSoFar++;
                                continue;
                            }

                            var product = products.FirstOrDefault(x => x.Sku.Trim().ToLower().Equals(erpStock.Sku.Trim().ToLower()));
                            if (product == null || product.Id == 0)
                            {
                                totalNotSyncedSoFar++;
                                continue;
                            }

                            if (!string.IsNullOrEmpty(erpStock.WarehouseNameOrCode))
                            {
                                if (mapsByCode.TryGetValue(erpStock.WarehouseNameOrCode.Trim(), out var codeMap) && codeMap.NopWarehouseId > 0)
                                {
                                    var inventory = inventories.Find(x => x.ProductId == product.Id && x.WarehouseId == codeMap.NopWarehouseId);
                                    if (inventory == null)
                                    {
                                        inventory = new ProductWarehouseInventory
                                        {
                                            ProductId = product.Id,
                                            WarehouseId = codeMap.NopWarehouseId,
                                            StockQuantity = (int)erpStock.QuantityOnHand,
                                            ReservedQuantity = 0
                                        };
                                        pwiToInsert.Add(inventory);

                                        var stockQuantityHistory = new StockQuantityHistory();
                                        stockQuantityHistory.QuantityAdjustment = (int)erpStock.QuantityOnHand;
                                        stockQuantityHistory.StockQuantity = (int)erpStock.QuantityOnHand;
                                        stockQuantityHistory.CreatedOnUtc = DateTime.UtcNow;
                                        stockQuantityHistory.ProductId = inventory.ProductId;
                                        stockQuantityHistory.WarehouseId = codeMap.NopWarehouseId;
                                        stockQuantityHistory.Message = $"Product Stock updated. The stock quantity has been updated by Erp Integration.";
                                        stockQuantityHistoriesToInsert.Add(stockQuantityHistory);
                                    }
                                    else
                                    {
                                        if (!propsToSkip.Contains(nameof(ErpStockDataModel.QuantityOnHand)))
                                        {
                                            if (inventory.StockQuantity != (int)erpStock.QuantityOnHand || inventory.ReservedQuantity > 0)
                                            {
                                                var quantityAdjustment = (int)erpStock.QuantityOnHand - inventory.StockQuantity;

                                                inventory.ReservedQuantity = 0;
                                                inventory.StockQuantity = (int)erpStock.QuantityOnHand;
                                                pwiToUpdate.Add(inventory);

                                                var stockQuantityHistory = new StockQuantityHistory();
                                                stockQuantityHistory.QuantityAdjustment = quantityAdjustment;
                                                stockQuantityHistory.StockQuantity = (int)erpStock.QuantityOnHand;
                                                stockQuantityHistory.CreatedOnUtc = DateTime.UtcNow;
                                                stockQuantityHistory.ProductId = inventory.ProductId;
                                                stockQuantityHistory.WarehouseId = codeMap.NopWarehouseId;
                                                stockQuantityHistory.Message = $"Product Stock updated. The stock quantity has been updated by Erp Integration.";
                                                stockQuantityHistoriesToInsert.Add(stockQuantityHistory);
                                            }
                                        }
                                    }

                                    product.StockQuantity = 0;
                                    product.ManageInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                                    product.UseMultipleWarehouses = true;
                                }
                            }
                            else
                            {
                                product.ManageInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                                product.StockQuantity = Convert.ToInt32(Math.Min(Math.Max(Math.Round(erpStock.QuantityOnHand ?? 0), int.MinValue), int.MaxValue));
                                product.UseMultipleWarehouses = false;
                            }
                            lastErpProductStockSynced = product.Sku;
                            totalSyncedSoFar++;
                        }

                        await _erpProductService.UpdateProductsAsync(products);
                        await _erpProductService.UpdateBulkProductWarehouseInventoryAsync(pwiToUpdate);
                        await _erpProductService.InsertBulkProductWarehouseInventoryAsync(pwiToInsert);
                        await _erpProductService.InsertBulkStockQuantityHistoryAsync(stockQuantityHistoriesToInsert);

                        pwiToUpdate.Clear();
                        pwiToInsert.Clear();
                        stockQuantityHistoriesToInsert.Clear();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                                ErpSyncLevel.Stock,
                                "The Erp Stock sync run is cancelled. " +
                                (!string.IsNullOrWhiteSpace(lastErpProductStockSynced) ?
                                $"The last synced Stock of Product : {lastErpProductStockSynced}. " : string.Empty) +
                                $"Total product stock synced so far: {totalSyncedSoFar}, " +
                                $"And total not synced due to invalid data or product not found: {totalNotSyncedSoFar}");

                            return false;
                        }

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                            ErpSyncLevel.Stock,
                            (!string.IsNullOrWhiteSpace(lastErpProductStockSynced) ?
                            $"The last synced Stock of Product : {lastErpProductStockSynced} in this batch. " : string.Empty) +
                            $"Total product stock synced so far: {totalSyncedSoFar}, " +
                            $"And total not synced due to invalid data or product not found: {totalNotSyncedSoFar}");
                    }

                    if (!isError)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                            ErpSyncLevel.Stock,
                            $"Erp Stock sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}.");
                    }
                    else
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                            ErpSyncLevel.Stock,
                            $"Erp Stock sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}.");
                    }

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                        ErpSyncLevel.Stock,
                        (!string.IsNullOrWhiteSpace(lastErpProductStockSynced) ?
                        $"The last synced Stock of Product: {lastErpProductStockSynced} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                        $"Total product stock synced so far: {totalSyncedSoFar}, " +
                        $"And total not synced due to invalid data or product not found: {totalNotSyncedSoFar}");

                    salesOrg.LastErpStockSyncTimeOnUtc = DateTime.UtcNow;
                    await _erpSalesOrgService.UpdateErpSalesOrgAsync(salesOrg);
                }
            }

            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                "Erp Stock Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                "Erp Stock Sync ended.");

            return false;
        }
    }

    #endregion
}
