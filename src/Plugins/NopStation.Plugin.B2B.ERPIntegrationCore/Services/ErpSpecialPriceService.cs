using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Catalog;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpSpecialPriceService : IErpSpecialPriceService
{
    #region Fields

    private readonly IRepository<ErpSpecialPrice> _erpSpecialPriceRepository;
    private readonly IRepository<ErpAccount> _erpAccountRepository;
    protected readonly IStaticCacheManager _staticCacheManager;
    private readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public ErpSpecialPriceService(IRepository<ErpSpecialPrice> erpSpecialPriceRepository,
        IRepository<ErpAccount> erpAccountRepository,
        IStaticCacheManager staticCacheManager,
        INopDataProvider nopDataProvider)
    {
        _erpSpecialPriceRepository = erpSpecialPriceRepository;
        _erpAccountRepository = erpAccountRepository;
        _staticCacheManager = staticCacheManager;
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
    {
        await _erpSpecialPriceRepository.InsertAsync(erpSpecialPrice);
    }

    public async Task InsertErpSpecialPricesAsync(List<ErpSpecialPrice> erpSpecialPrices)
    {
        await _erpSpecialPriceRepository.InsertAsync(erpSpecialPrices);
    }

    public async Task UpdateErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
    {
        await _erpSpecialPriceRepository.UpdateAsync(erpSpecialPrice);
    }

    public async Task UpdateErpSpecialPricesAsync(List<ErpSpecialPrice> erpSpecialPrices)
    {
        await _erpSpecialPriceRepository.UpdateAsync(erpSpecialPrices);
    }

    #endregion

    #region Delete

    private async Task DeleteErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
    {
        await _erpSpecialPriceRepository.DeleteAsync(erpSpecialPrice);
    }

    public async Task DeleteErpSpecialPriceByIdAsync(int id)
    {
        var erpSpecialPrice = await GetErpSpecialPriceByIdAsync(id);
        if (erpSpecialPrice != null)
        {
            await DeleteErpSpecialPriceAsync(erpSpecialPrice);
        }
    }

    public async Task DeleteSpecialPricesNotUpdatedSinceSyncStart(int accountId, DateTime syncStartTime)
    {
        if (syncStartTime == DateTime.MinValue)
            return;

        var connectionString = new SqlConnectionStringBuilder(DataSettingsManager.LoadSettings().ConnectionString);

        var sqlCommand = $"Update [{connectionString.InitialCatalog}].[dbo].[Erp_Special_Price] Set [Deleted] = 1 Where [ErpAccount_Id] = {accountId} and ([UpdatedOnUtc] < '{syncStartTime:yyyy-MM-dd HH:mm:ss}' or [UpdatedOnUtc] is null)";

        await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);
    }

    #endregion

    #region Read

    public async Task<ErpSpecialPrice> GetErpSpecialPriceByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _erpSpecialPriceRepository.GetByIdAsync(id, cache => default, includeDeleted: false);
    }

    public async Task<IPagedList<ErpSpecialPrice>> GetAllErpSpecialPricesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool? overridePublished = null, int productId = 0, int accountId = 0, bool onlyIncludeActiveErpAccountsMappedPrices = false)
    {
        var erpSpecialPrice = await _erpSpecialPriceRepository.GetAllPagedAsync(query =>
        {
            if (productId > 0)
                query = query.Where(ei => ei.NopProductId == productId);

            if (accountId > 0)
                query = query.Where(ei => ei.ErpAccountId == accountId);

            if (onlyIncludeActiveErpAccountsMappedPrices)
            {
                query = query.Join(_erpAccountRepository.Table,
                    specialPrice => specialPrice.ErpAccountId,
                    account => account.Id,
                    (specialPrice, account) => new { SpecialPrice = specialPrice, Account = account })
                .Where(joined => joined.Account.IsActive)
                .Select(joined => joined.SpecialPrice);
            }

            query = query.OrderBy(ei => ei.Id);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount, includeDeleted: false);

        return erpSpecialPrice;
    }

    public async Task<IList<ErpSpecialPrice>> GetErpSpecialPricesByErpAccountIdAsync(int erpAcoountId)
    {
        if (erpAcoountId == 0)
            return null;

        var erpSpecialPrices = await _erpSpecialPriceRepository.GetAllAsync(query =>
        {
            query = query.Where(ei => ei.ErpAccountId == erpAcoountId);
            query = query.OrderBy(ei => ei.Id);
            return query;
        }, includeDeleted: false);

        return erpSpecialPrices;
    }

    public async Task<IList<ErpSpecialPrice>> GetErpSpecialPricesByNopProductIdAsync(int nopProductId)
    {
        if (nopProductId == 0)
            return null;

        var erpSpecialPrices = await _erpSpecialPriceRepository.GetAllAsync(query =>
        {
            return from sp in query
                   where sp.NopProductId == nopProductId
                   orderby sp.Id descending
                   select sp;
        }, cache => cache.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpProductPricingSpecialPriceByProductCacheKey, nopProductId), includeDeleted: false);

        return erpSpecialPrices;
    }

    public async Task<ErpSpecialPrice> GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(int accountId, int nopProductId)
    {
        if (accountId == 0 || nopProductId == 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpProductPricingSpecialPriceByProductIdAndAccountCacheKey, nopProductId, accountId);

        var query = _erpSpecialPriceRepository.Table.Where(b => b.ErpAccountId == accountId && b.NopProductId == nopProductId && !b.Deleted);

        return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
    }

    public async Task<bool> CheckAnySpecialPriceExistWithAccountIdAndProductId(int accountId, int productId)
    {
        if (accountId == 0 || productId == 0)
            return false;

        return await GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(accountId, productId) != null;
    }

    #endregion

    #endregion
}
