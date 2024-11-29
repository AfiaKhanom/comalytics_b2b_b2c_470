using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpAccountService : IErpAccountService
{
    #region Fields

    private readonly IRepository<ErpAccount> _erpAccountRepository;
    private readonly IRepository<ErpSalesRepErpAccountMap> _erpSalesRepErpAccountMapRepository;
    private readonly IRepository<ErpShiptoAddressErpAccountMap> _erpShiptoAddressErpAccountMapRepository;
    private readonly INopDataProvider _nopDataProvider;
    private readonly IRepository<ErpNopUser> _erpNopUserRepository;
    private readonly IRepository<ErpNopUserAccountMap> _erpNopUserAccountMapRepository;
    private readonly IRepository<Address> _addressRepository;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IRepository<ErpOrderAdditionalData> _erpOrderAdditionalRepository;

    #endregion

    #region Ctor

    public ErpAccountService(IRepository<ErpAccount> erpAccountRepository,
        IRepository<ErpOrderAdditionalData> erpOrderAdditionalRepository,
        IRepository<Address> addressRepository,
        IErpNopUserService erpNopUserService,
        IRepository<ErpSalesRepErpAccountMap> erpSalesRepErpAccountMapRepository,
        IRepository<ErpShiptoAddressErpAccountMap> erpShiptoAddressErpAccountMapRepository,
        INopDataProvider nopDataProvider,
        IRepository<ErpNopUser> erpNopUserRepository,
        IRepository<ErpNopUserAccountMap> erpNopUserAccountMapRepository)
    {
        _erpAccountRepository = erpAccountRepository;
        _addressRepository = addressRepository;
        _erpNopUserService = erpNopUserService;
        _erpSalesRepErpAccountMapRepository = erpSalesRepErpAccountMapRepository;
        _erpShiptoAddressErpAccountMapRepository = erpShiptoAddressErpAccountMapRepository;
        _nopDataProvider = nopDataProvider;
        _erpNopUserRepository = erpNopUserRepository;
        _erpNopUserAccountMapRepository = erpNopUserAccountMapRepository;
        _erpOrderAdditionalRepository = erpOrderAdditionalRepository;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpAccountAsync(ErpAccount erpAccount)
    {
        await _erpAccountRepository.InsertAsync(erpAccount);
    }

    public async Task InsertErpAccountsAsync(List<ErpAccount> erpAccounts)
    {
        await _erpAccountRepository.InsertAsync(erpAccounts);
    }

    public async Task InsertSalesRepErpAccountAsync(ErpSalesRepErpAccountMap erpSalesRepErpAccount)
    {
        await _erpSalesRepErpAccountMapRepository.InsertAsync(erpSalesRepErpAccount);
    }

    public async Task UpdateErpAccountAsync(ErpAccount erpAccount)
    {
        await _erpAccountRepository.UpdateAsync(erpAccount);
    }

    public async Task UpdateErpAccountsAsync(List<ErpAccount> erpAccounts)
    {
        await _erpAccountRepository.UpdateAsync(erpAccounts);
    }

    #endregion

    #region Delete

    private async Task DeleteErpAccountAsync(ErpAccount erpAccount)
    {
        //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
        erpAccount.IsDeleted = true;
        await _erpAccountRepository.UpdateAsync(erpAccount);
    }

    public async Task DeleteErpAccountByIdAsync(int id)
    {
        var erpAccount = await GetErpAccountByIdAsync(id);
        if (erpAccount != null)
        {
            await DeleteErpAccountAsync(erpAccount);
        }
    }
    public async Task DeleteErpSalesRepErpAccountMapAsync(ErpSalesRepErpAccountMap salesRepErpAccountMap)
    {
        await _erpSalesRepErpAccountMapRepository.DeleteAsync(salesRepErpAccountMap);
    }

    #endregion

    #region Read

    /// <summary>
    /// Gets an ErpAccount by Id
    /// </summary>
    /// <param name="id">ErpAccount identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ErpAccount
    /// </returns>
    public async Task<ErpAccount> GetErpAccountByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _erpAccountRepository.GetByIdAsync(id, cache => default);
    }

    public async Task<ErpAccount> GetErpAccountByErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
    {
        if (erpShipToAddress == null)
            return null;
        var query = from erpAccount in _erpAccountRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table on erpAccount.Id equals cam.ErpAccountId
                    where cam.ErpShiptoAddressId == erpShipToAddress.Id
                    select erpAccount;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<ErpSalesRepErpAccountMap> GetErpSalesRepErpAccountMapByIdAsync(int salesRepId, int? erpAccountId)
    {
        // Check if both ids are provided and valid
        if (salesRepId <= 0 || (erpAccountId.HasValue && erpAccountId <= 0))
        {
            return null;
        }
        // Fetch the record based on salesRepId and erpAccountId
        var record = await _erpSalesRepErpAccountMapRepository.Table.FirstOrDefaultAsync(x => x.ErpSalesRepId == salesRepId && x.ErpAccountId == erpAccountId);
        return record;
    }

    /// <summary>
    /// Gets an ErpAccount by Id if it is active
    /// </summary>
    /// <param name="id">ErpAccount identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ErpAccount if it is activ
    /// </returns>
    public async Task<ErpAccount> GetErpAccountByIdWithActiveAsync(int id)
    {
        if (id == 0)
            return null;

        var erpAccount = await _erpAccountRepository.GetByIdAsync(id, cache => default);

        if (erpAccount == null || !erpAccount.IsActive)
            return null;

        return erpAccount;
    }

    public async Task<IList<ErpAccount>> GetAllErpAccountsAsync()
    {
        var erpAccounts = await _erpAccountRepository.GetAllAsync(query =>
        {
            query = query.Where(v => v.IsActive && !v.IsDeleted);
            query = query.OrderBy(ea => ea.Id);
            return query;
        });

        return erpAccounts;
    }

    /// <summary>
    /// Gets all ErpAccounts
    /// </summary>
    /// <param name="pageIndex">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains all the ErpAccounts
    /// </returns>
    public async Task<IPagedList<ErpAccount>> GetAllErpAccountsAsync(int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool? showHidden = null,
        bool getOnlyTotalCount = false,
        string erpAccontNo = null,
        int salesOrgId = 0,
        string email = null,
        string accountName = null,
        int erpAccountStatusTypeId = 0,
        bool filterDeleted = true)
    {
        var erpAccounts = await _erpAccountRepository.GetAllPagedAsync(query =>
        {
            // showHidden is null for getting all, true for only actives and false for only inactives
            if (showHidden.HasValue)
            {
                if (!showHidden.Value)
                    query = query.Where(v => v.IsActive);
                else
                    query = query.Where(v => !v.IsActive);
            }

            if (erpAccountStatusTypeId > 0)
                query = query.Where(c => c.ErpAccountStatusTypeId.Equals(erpAccountStatusTypeId));

            if (!string.IsNullOrWhiteSpace(erpAccontNo))
                query = query.Where(c => c.AccountNumber.Contains(erpAccontNo));

            if (salesOrgId > 0)
                query = query.Where(c => c.ErpSalesOrgId.Equals(salesOrgId));

            if (!string.IsNullOrWhiteSpace(accountName))
                query = query.Where(c => c.AccountName.Contains(accountName));

            if (filterDeleted)
                query = query.Where(v => !v.IsDeleted);

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Join(_addressRepository.Table, x => x.BillingAddressId, y => y.Id,
                        (x, y) => new { ErpAccount = x, Address = y })
                    .Where(z => z.Address.Email.Contains(email))
                    .Select(z => z.ErpAccount)
                    .Distinct();
            }

            query = query.OrderBy(ea => ea.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpAccounts;
    }

    public async Task<IList<ErpSalesRepErpAccountMap>> GetAllErpAccountsBySalesRepIdAsync(string erpSalesRepId = null)
    {
        var erpAccounts = await _erpSalesRepErpAccountMapRepository.GetAllAsync(query =>
        {
            if (!string.IsNullOrWhiteSpace(erpSalesRepId))
                query = query.Where(c => c.ErpSalesRepId.Equals(Convert.ToInt32(erpSalesRepId)));

            query = query.OrderBy(ea => ea.Id);
            return query;

        });

        return erpAccounts;
    }

    /// <summary>
    /// Gets all ErpAccounts
    /// </summary>
    /// <param name="pageIndex">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains all the ErpAccounts
    /// </returns>
    public async Task<IPagedList<ErpAccount>> GetAllErpAccountsByIdsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false,
        bool getOnlyTotalCount = false, List<int> accountIds = null, string email = "")
    {
        var erpAccounts = await _erpAccountRepository.GetAllPagedAsync(query =>
        {
            if (!showHidden)
                query = query.Where(v => v.IsActive);

            query = query.Where(c => accountIds.Contains(c.Id));

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Join(_addressRepository.Table, x => x.BillingAddressId, y => y.Id,
                        (x, y) => new { ErpAccount = x, Address = y })
                    .Where(z => z.Address.Email.Contains(email))
                    .Select(z => z.ErpAccount)
                    .Distinct();
            }

            query = query.OrderBy(ea => ea.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpAccounts;
    }

    public async Task<IList<ErpAccount>> GetErpAccountsOfOnlyActiveErpNopUsersAsync(int salesOrgId = 0)
    {
        return await (from eaMap in _erpNopUserAccountMapRepository.Table
                      join ea in _erpAccountRepository.Table on eaMap.ErpAccountId equals ea.Id
                      join enu in _erpNopUserRepository.Table on eaMap.ErpUserId equals enu.Id
                      where ea.ErpSalesOrgId == salesOrgId && ea.IsActive && !ea.IsDeleted && !enu.IsDeleted && enu.IsActive
                      select ea).Distinct().ToListAsync();
    }

    public async Task<ErpAccount> GetErpAccountByErpAccountNumberAsync(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber))
            return null;


        var query = from c in _erpAccountRepository.Table
                    where c.AccountNumber == accountNumber
                    orderby c.Id
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task<ErpAccount> GetActiveErpAccountByCustomerIdAsync(int customerId)
    {
        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customerId);
        if (erpNopUser != null && !erpNopUser.IsDeleted && erpNopUser.IsActive)
        {
            var erpAccount = await GetErpAccountByIdWithActiveAsync(erpNopUser.ErpAccountId);
            if (erpAccount != null)
                return erpAccount;
        }

        return null;
    }

    public async Task InActiveAllOldAccount(DateTime syncStartTime)
    {
        if (syncStartTime == DateTime.MinValue)
            return;

        var connectionString = new SqlConnectionStringBuilder(DataSettingsManager.LoadSettings().ConnectionString);

        var sqlCommand = $"Update [{connectionString.InitialCatalog}].[dbo].[Erp_Account] Set [IsActive] = 0 Where [UpdatedOnUtc] < '{syncStartTime:yyyy-MM-dd HH:mm:ss}'";

        await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);
    }

    public async Task<IList<ErpAccount>> GetAllErpAccountsBySaleOrgIdAsync(int salesOrgId)
    {
        if (salesOrgId < 1)
            return new List<ErpAccount>();

        return await _erpAccountRepository.Table.Where(x => x.ErpSalesOrgId == salesOrgId && x.IsActive && !x.IsDeleted).ToListAsync();
    }

    #endregion

    #endregion
}
