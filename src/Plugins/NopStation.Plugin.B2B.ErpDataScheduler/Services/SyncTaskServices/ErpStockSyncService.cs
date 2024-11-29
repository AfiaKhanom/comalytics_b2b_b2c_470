using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Common;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpStockSyncService : IErpStockSyncService
{
    #region Fields

    private readonly IProductService _productService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpProductService _erpProductService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpWarehouseAdditionalDataService _erpWarehouseAdditionalDataService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ErpDataSchedulerSettings _erpDataSchedulerSettings;

    #endregion

    #region Ctor

    public ErpStockSyncService(IProductService productService,
        ISyncLogService erpSyncLogService,
        IErpProductService erpProductService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IErpSalesOrgService erpSalesOrgService,
        IErpWarehouseAdditionalDataService erpWarehouseAdditionalDataService,
        IGenericAttributeService genericAttributeService,
        ErpDataSchedulerSettings erpDataSchedulerSettings)
    {
        _productService = productService;
        _erpSyncLogService = erpSyncLogService;
        _erpProductService = erpProductService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpWarehouseAdditionalDataService = erpWarehouseAdditionalDataService;
        _genericAttributeService = genericAttributeService;
        _erpDataSchedulerSettings = erpDataSchedulerSettings;
    }

    #endregion

    #region Method

    public async virtual Task<bool> IsErpStockSyncSuccessfulAsync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                "No integration method found.");

            return false;
        }

        try
        {
            #region Data collection

            var listOfSalesOrgs = new List<ErpSalesOrg>();
            var salesOrgCode = await erpIntegrationPlugin.GetSalesOrgCodeFromIntegrationSettings();

            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                var salesOrg = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(code: salesOrgCode)).FirstOrDefault();

                if (salesOrg == null)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                    ErpSyncLevel.Stock,
                    $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpStockSyncTaskName}.");

                    return false;
                }
                else
                {
                    listOfSalesOrgs.Add(salesOrg);
                }
            }
            else
            {
                var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();

                if (salesOrgs.Any())
                {
                    listOfSalesOrgs.AddRange(salesOrgs);
                }
            }

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                "Erp Stock Sync started.");

            foreach (var salesOrg in listOfSalesOrgs)
            {
                var start = "0";
                var isError = false;
                var lastErpProductStockSynced = string.Empty;
                var totalSyncedSoFar = 0;

                var lastStockRefreshTime = await _genericAttributeService.GetAttributeAsync<DateTime?>(salesOrg, B2BB2CFeaturesDefaults.LastStockSyncDateTime);
                if (lastStockRefreshTime == null || lastStockRefreshTime == DateTime.MinValue)
                {
                    lastStockRefreshTime = _erpDataSchedulerSettings.SyncFromDate ?? null;
                }

                await _genericAttributeService.SaveAttributeAsync(salesOrg, B2BB2CFeaturesDefaults.LastStockSyncDateTime, DateTime.UtcNow);

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        DateFrom = lastStockRefreshTime,
                        Location = salesOrg.Code
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

                        break;
                    }
                    else if (response.Data is null)
                    {
                        isError = false;
                        break;
                    }

                    start = response.ErpResponseModel.Next;

                    var pwiToInsert = new List<ProductWarehouseInventory>();
                    var pwiToUpdate = new List<ProductWarehouseInventory>();
                    var stockQuantityHistoriesToInsert = new List<StockQuantityHistory>();

                    var products = await (await _productService.GetProductsBySkuAsync(response.Data.Where(x => !string.IsNullOrWhiteSpace(x.Sku)).Select(x => x.Sku).ToArray())).ToListAsync();
                    if (products is null || products.Count == 0)
                    {
                        isError = false;
                        break;
                    }

                    var erpSalesOrgWarehouse = await _erpWarehouseAdditionalDataService.GetSaleOrgWarehousebySalesOrgIdAsync(salesOrg.Id);
                    var inventories = new List<ProductWarehouseInventory>();
                    if (erpSalesOrgWarehouse != null && erpSalesOrgWarehouse.Count > 0)
                    {
                        inventories = await (await _erpProductService.GetProductWarehouseInventoryByProductIdsAndNopWarehouseIdsAsync(products.Select(x => x.Id).ToArray(), erpSalesOrgWarehouse.Select(x => x.NopWarehouseId).ToArray())).ToListAsync();
                    }

                    var responseData = response.Data
                            .Where(x => !string.IsNullOrWhiteSpace(x.Sku) && !string.IsNullOrWhiteSpace(x.WarehouseNameOrCode))
                            .GroupBy(x => new { x.Sku, x.WarehouseNameOrCode })
                            .Select(g => g.Last());

                    foreach (var erpStock in responseData)
                    {
                        if (string.IsNullOrWhiteSpace(erpStock.Sku) || !erpStock.QuantityOnHand.HasValue)
                            continue;

                        var product = products.Find(x => x.Sku.Equals(erpStock.Sku));
                        if (product == null || product.Id == 0)
                            continue;

                        if (!string.IsNullOrEmpty(erpStock.WarehouseNameOrCode))
                        {
                            var erpWarehouse = await _erpWarehouseAdditionalDataService.GetErpWarehouseAdditionalDataByCodeAsync(erpStock.WarehouseNameOrCode.Trim());

                            if (erpWarehouse != null && erpWarehouse.NopWarehouseId > 0)
                            {
                                var inventory = inventories.Find(x => x.ProductId == product.Id && x.WarehouseId == erpWarehouse.NopWarehouseId);
                                if (inventory == null)
                                {
                                    inventory = new ProductWarehouseInventory
                                    {
                                        ProductId = product.Id,
                                        WarehouseId = erpWarehouse.NopWarehouseId,
                                        StockQuantity = (int)erpStock.QuantityOnHand,
                                        ReservedQuantity = 0
                                    };
                                    pwiToInsert.Add(inventory);

                                    var stockQuantityHistory = new StockQuantityHistory();
                                    stockQuantityHistory.QuantityAdjustment = (int)erpStock.QuantityOnHand;
                                    stockQuantityHistory.StockQuantity = (int)erpStock.QuantityOnHand;
                                    stockQuantityHistory.CreatedOnUtc = DateTime.UtcNow;
                                    stockQuantityHistory.ProductId = inventory.ProductId;
                                    stockQuantityHistory.WarehouseId = erpWarehouse.NopWarehouseId;
                                    stockQuantityHistory.Message = $"Product Stock updated. The stock quantity has been updated by Integration.";
                                    stockQuantityHistoriesToInsert.Add(stockQuantityHistory);
                                }
                                else
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
                                        stockQuantityHistory.WarehouseId = erpWarehouse.NopWarehouseId;
                                        stockQuantityHistory.Message = $"Product Stock updated. The stock quantity has been updated by Integration.";
                                        stockQuantityHistoriesToInsert.Add(stockQuantityHistory);
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

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                        ErpSyncLevel.Stock,
                        (!string.IsNullOrWhiteSpace(lastErpProductStockSynced) ? $"The last synced Stock of Erp Product : {lastErpProductStockSynced} in this batch. " : string.Empty) + $"Total product stock synced so far: {totalSyncedSoFar}");
                }

                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                        ErpSyncLevel.Stock,
                        $"Erp Stock sync successful for Sales Org: {salesOrg.Name}.");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                        ErpSyncLevel.Stock,
                        $"Erp Stock sync is partially or not successful for Sales Org: {salesOrg.Name}.");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                    ErpSyncLevel.Stock,
                    (!string.IsNullOrWhiteSpace(lastErpProductStockSynced) ? $"The last synced Stock of Erp Product : {lastErpProductStockSynced} in this batch. " : string.Empty) + $"Total product stock synced so far: {totalSyncedSoFar}");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                "Erp Stock Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                ex.Message,
                ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLevel.Stock,
                "Erp Stock Sync ended.");

            return false;
        }
    }

    #endregion
}