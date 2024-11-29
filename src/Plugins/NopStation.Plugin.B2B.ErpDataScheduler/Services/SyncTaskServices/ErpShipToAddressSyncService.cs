using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Directory;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpShipToAddressSyncService : IErpShipToAddressSyncService
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public ErpShipToAddressSyncService(IAddressService addressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpShipToAddressService erpShipToAddressService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _addressService = addressService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpShipToAddressService = erpShipToAddressService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Method

    public async virtual Task<bool> IsErpShipToAddressSyncSuccessfulAsync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
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
                    ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                    ErpSyncLevel.ShipToAddress,
                    $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName}.");

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

            var allCountries = await _countryService.GetAllCountriesAsync();
            var allStateProvinces = await _stateProvinceService.GetStateProvincesAsync();

            var erpShipToAddressUpdateList = new List<ErpShipToAddress>();
            var erpShipToAddressInsertList = new List<ErpShipToAddress>();
            var erpShiptoAddressErpAccountMapInsertList = new List<ErpShiptoAddressErpAccountMap>();

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                "Erp ShipToAddress Sync started.");

            foreach (var salesOrg in listOfSalesOrgs)
            {
                var oldErpAccounts = await _erpAccountService.GetAllErpAccountsBySaleOrgIdAsync(salesOrg.Id);

                if (oldErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLevel.ShipToAddress,
                        $"No Erp Accounts found with the Sales org : {salesOrg.Name}");

                    continue;
                }

                var lastSyncedErpShipToAddressShipToCode = string.Empty;
                var lastErpShipToAddressSyncedOfErpAccount = "";
                var totalSyncedSoFar = 0;
                var isError = false;
                var lastErrorMessage = "";
                var countryId = 0;
                var stateProvinceId = 0;

                foreach (var erpAccount in oldErpAccounts)
                {
                    var start = "0";
                    var totalSyncedSoFarForThisAccount = 0;

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            AccountNumber = erpAccount.AccountNumber,
                            Location = salesOrg.Code
                        };

                        var response = await erpIntegrationPlugin
                            .GetShipToAddressByAccountNumberFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError)
                        {
                            isError = true;
                            lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";
                            break;
                        }
                        else if (response.Data is null)
                        {
                            isError = false;
                            break;
                        }

                        start = response.ErpResponseModel.Next;

                        var responseData = response.Data
                            .Where(x => !string.IsNullOrWhiteSpace(x.ShipToCode))
                            .GroupBy(x => x.ShipToCode.Trim())
                            .Select(g => g.Last());

                        foreach (var erpShipToAddress in responseData)
                        {
                            var oldShipToAddressByThisAccount = await _erpShipToAddressService
                                .GetErpShipToAddressByShiptocodeAndAccountIdAsync(shipToCode: erpShipToAddress.ShipToCode, erpAccountId: erpAccount.Id);

                            var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0);

                            countryId = allCountries.FirstOrDefault(x =>
                                !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpShipToAddress.Country)
                                || !string.IsNullOrWhiteSpace(x.TwoLetterIsoCode) && x.TwoLetterIsoCode.Equals(erpShipToAddress.Country)
                                || !string.IsNullOrWhiteSpace(x.ThreeLetterIsoCode) && x.ThreeLetterIsoCode.Equals(erpShipToAddress.Country))?.Id
                                ?? _b2BB2CFeaturesSettings.DefaultCountryId;

                            stateProvinceId = allStateProvinces.FirstOrDefault(x => x.CountryId == countryId
                                && (!string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpShipToAddress.StateProvince) ||
                                !string.IsNullOrWhiteSpace(x.Abbreviation) && x.Abbreviation.Equals(erpShipToAddress.StateProvince)))?.Id ?? 0;

                            if (address is null)
                            {
                                address = new Address();
                                address.FirstName = erpShipToAddress.ShipToName;
                                address.Email = erpShipToAddress.EmailAddress;
                                address.Company = erpShipToAddress.Company;
                                address.CountryId = countryId;
                                address.City = erpShipToAddress.City;
                                address.County = erpShipToAddress.County;
                                address.Address1 = erpShipToAddress.Address1;
                                address.Address2 = erpShipToAddress.Address2;
                                address.ZipPostalCode = erpShipToAddress.ZipPostalCode;
                                address.StateProvinceId = stateProvinceId;
                                address.PhoneNumber = erpShipToAddress.PhoneNumber;
                                address.FaxNumber = string.Empty;

                                address.CreatedOnUtc = DateTime.UtcNow;
                                await _addressService.InsertAddressAsync(address);
                            }
                            else
                            {
                                address.FirstName = erpShipToAddress.ShipToName;
                                address.Email = erpShipToAddress.EmailAddress;
                                address.Company = erpShipToAddress.Company;
                                address.CountryId = countryId;
                                address.City = erpShipToAddress.City;
                                address.County = erpShipToAddress.County;
                                address.Address1 = erpShipToAddress.Address1;
                                address.Address2 = erpShipToAddress.Address2;
                                address.ZipPostalCode = erpShipToAddress.ZipPostalCode;
                                address.StateProvinceId = stateProvinceId;
                                address.PhoneNumber = erpShipToAddress.PhoneNumber;
                                address.FaxNumber = string.Empty;

                                await _addressService.UpdateAddressAsync(address);
                            }

                            if (oldShipToAddressByThisAccount is null)
                            {
                                oldShipToAddressByThisAccount = new ErpShipToAddress();
                                oldShipToAddressByThisAccount.ShipToCode = erpShipToAddress.ShipToCode.Trim();
                                oldShipToAddressByThisAccount.ShipToName = erpShipToAddress.ShipToName.Trim();
                                oldShipToAddressByThisAccount.Suburb = erpShipToAddress.Suburb;
                                oldShipToAddressByThisAccount.ProvinceCode = erpShipToAddress.StateProvince;
                                oldShipToAddressByThisAccount.DeliveryNotes = erpShipToAddress.DeliveryNotes;
                                oldShipToAddressByThisAccount.EmailAddresses = erpShipToAddress.EmailAddress;
                                oldShipToAddressByThisAccount.RepNumber = erpShipToAddress.RepNumber;
                                oldShipToAddressByThisAccount.RepPhoneNumber = erpShipToAddress.RepPhoneNumber;
                                oldShipToAddressByThisAccount.RepEmail = erpShipToAddress.RepEmail;
                                oldShipToAddressByThisAccount.RepFullName = erpShipToAddress.RepFullName;
                                oldShipToAddressByThisAccount.AddressId = address.Id;
                                oldShipToAddressByThisAccount.IsActive = erpAccount.IsActive;
                                oldShipToAddressByThisAccount.CreatedOnUtc = DateTime.UtcNow;
                                oldShipToAddressByThisAccount.CreatedById = 1;
                                oldShipToAddressByThisAccount.UpdatedOnUtc = DateTime.UtcNow;
                                oldShipToAddressByThisAccount.UpdatedById = 1;
                                oldShipToAddressByThisAccount.LastShipToAddressSyncDate = DateTime.UtcNow;

                                erpShipToAddressInsertList.Add(oldShipToAddressByThisAccount);
                            }
                            else
                            {
                                oldShipToAddressByThisAccount.ShipToCode = erpShipToAddress.ShipToCode.Trim();
                                oldShipToAddressByThisAccount.ShipToName = erpShipToAddress.ShipToName.Trim();
                                oldShipToAddressByThisAccount.Suburb = erpShipToAddress.Suburb;
                                oldShipToAddressByThisAccount.ProvinceCode = erpShipToAddress.StateProvince;
                                oldShipToAddressByThisAccount.DeliveryNotes = erpShipToAddress.DeliveryNotes;
                                oldShipToAddressByThisAccount.EmailAddresses = erpShipToAddress.EmailAddress;
                                oldShipToAddressByThisAccount.RepNumber = erpShipToAddress.RepNumber;
                                oldShipToAddressByThisAccount.RepPhoneNumber = erpShipToAddress.RepPhoneNumber;
                                oldShipToAddressByThisAccount.RepEmail = erpShipToAddress.RepEmail;
                                oldShipToAddressByThisAccount.RepFullName = erpShipToAddress.RepFullName;
                                oldShipToAddressByThisAccount.AddressId = address.Id;
                                oldShipToAddressByThisAccount.IsActive = erpAccount.IsActive;
                                oldShipToAddressByThisAccount.UpdatedOnUtc = DateTime.UtcNow;
                                oldShipToAddressByThisAccount.UpdatedById = 1;
                                oldShipToAddressByThisAccount.LastShipToAddressSyncDate = DateTime.UtcNow;

                                erpShipToAddressUpdateList.Add(oldShipToAddressByThisAccount);
                                if (await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(oldShipToAddressByThisAccount.Id) == null)
                                {
                                    erpShiptoAddressErpAccountMapInsertList.Add(new ErpShiptoAddressErpAccountMap
                                    {
                                        ErpAccountId = erpAccount.Id,
                                        ErpShiptoAddressId = oldShipToAddressByThisAccount.Id
                                    });
                                }
                            }

                            lastSyncedErpShipToAddressShipToCode = oldShipToAddressByThisAccount.ShipToCode;
                            lastErpShipToAddressSyncedOfErpAccount = erpAccount.AccountNumber;
                            totalSyncedSoFar++;
                            totalSyncedSoFarForThisAccount++;
                        }

                        if (erpShipToAddressInsertList.Count != 0)
                        {
                            await _erpShipToAddressService.InsertErpShipToAddressesAsync(erpShipToAddressInsertList);

                            foreach (var erpShipToAddress in erpShipToAddressInsertList)
                            {
                                erpShiptoAddressErpAccountMapInsertList.Add(new ErpShiptoAddressErpAccountMap
                                {
                                    ErpAccountId = erpAccount.Id,
                                    ErpShiptoAddressId = erpShipToAddress.Id
                                });
                            }
                            erpShipToAddressInsertList.Clear();
                        }

                        if (erpShipToAddressUpdateList.Count != 0)
                        {
                            await _erpShipToAddressService.UpdateErpShipToAddressesAsync(erpShipToAddressUpdateList);
                            await _erpDataClearCacheService.ClearCacheOfEntities(erpShipToAddressUpdateList);
                            erpShipToAddressUpdateList.Clear();
                        }

                        if (erpShiptoAddressErpAccountMapInsertList.Count != 0)
                        {
                            await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapsAsync(erpShiptoAddressErpAccountMapInsertList);
                            erpShiptoAddressErpAccountMapInsertList.Clear();
                        }
                    }

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                    ErpSyncLevel.ShipToAddress,
                    $"Total {totalSyncedSoFarForThisAccount} Erp Ship To Addresses synced " +
                    $"for Erp Account: {erpAccount.AccountNumber} ({erpAccount.AccountName}) " +
                    $"Sales Org: {salesOrg.Name}");

                    #region Cache clear for this erp account

                    await _erpDataClearCacheService.ClearCacheOfEntity(erpAccount);

                    #endregion
                }
                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLevel.ShipToAddress,
                        $"Erp Ship to address sync successful for Sales Org: {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLevel.ShipToAddress,
                        $"Erp Ship to address sync is partially or not successful for Sales Org: {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                    ErpSyncLevel.ShipToAddress,
                    (!string.IsNullOrWhiteSpace(lastSyncedErpShipToAddressShipToCode) ? $"The last synced Erp Ship To Address: {lastSyncedErpShipToAddressShipToCode}, " +
                    $"of Erp Account: {lastErpShipToAddressSyncedOfErpAccount} for Sales Org: {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar}");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                "Erp ShipToAddress Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                ex.Message,
                ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                "Erp ShipToAddress Sync ended.");

            return false;
        }
    }

    #endregion
}