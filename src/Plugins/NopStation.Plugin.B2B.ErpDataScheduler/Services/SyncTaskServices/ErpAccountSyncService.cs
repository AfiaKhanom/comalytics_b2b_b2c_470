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

public class ErpAccountSyncService : IErpAccountSyncService
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private const string HIDE_STOCK_VALUES = "HideStockValues";
    private const int ALLOWED_STOCK_PERCENTAGE = 100;

    #endregion

    #region Ctor

    public ErpAccountSyncService(IAddressService addressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
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
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginManager = erpIntegrationPluginService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpAccountSyncSuccessfulAsync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                "No integration method found.");

            return false;
        }

        try
        {
            #region Data collections

            var listOfSalesOrgs = new List<ErpSalesOrg>();
            var salesOrgCode = await erpIntegrationPlugin.GetSalesOrgCodeFromIntegrationSettings();
            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                var salesOrg = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(code: salesOrgCode)).FirstOrDefault();

                if (salesOrg == null)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                        ErpSyncLevel.Account,
                        $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpAccountSyncTaskName}.");

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

            var allCountries = (await _countryService.GetAllCountriesAsync()).ToList();
            var allStateProvinces = (await _stateProvinceService.GetStateProvincesAsync()).ToList();
            var countryId = 0;
            var stateProvinceId = 0;
            var erpAccountUpdateList = new List<ErpAccount>();
            var erpAccountInsertList = new List<ErpAccount>();
            var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                "Erp Account Sync started.");

            foreach (var salesOrg in listOfSalesOrgs)
            {
                var oldErpAccounts = await _erpAccountService.GetAllErpAccountsAsync(salesOrgId: salesOrg.Id, filterDeleted: false);
                var isError = false;
                var start = "0";
                var lastSyncedErpAccountNumber = string.Empty;
                var totalSyncedSoFar = 0;

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        Location = salesOrg.Code
                    };

                    var response = await erpIntegrationPlugin.GetAccountsFromErpAsync(erpGetRequestModel);

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                            ErpSyncLevel.Account,
                            response.ErpResponseModel.ErrorShortMessage,
                            response.ErpResponseModel.ErrorFullMessage);
                        break;
                    }
                    else if (response.Data is null)
                    {
                        isError = false;
                        break;
                    }

                    start = response.ErpResponseModel.Next;

                    var responseData = response.Data
                        .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
                        .GroupBy(x => x.AccountNumber)
                        .Select(g => g.Last());

                    foreach (var erpAccount in responseData)
                    {
                        var oldErpAccount = oldErpAccounts.FirstOrDefault(x => x.AccountNumber == erpAccount.AccountNumber);

                        var address = await _addressService.GetAddressByIdAsync(oldErpAccount?.BillingAddressId ?? 0);

                        countryId = allCountries.FirstOrDefault(x =>
                                !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpAccount.Country)
                                || !string.IsNullOrWhiteSpace(x.TwoLetterIsoCode) && x.TwoLetterIsoCode.Equals(erpAccount.Country)
                                || !string.IsNullOrWhiteSpace(x.ThreeLetterIsoCode) && x.ThreeLetterIsoCode.Equals(erpAccount.Country))?.Id
                                ?? _b2BB2CFeaturesSettings.DefaultCountryId;

                        stateProvinceId = allStateProvinces.FirstOrDefault(x => x.CountryId == countryId
                            && (!string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpAccount.StateProvince) ||
                            !string.IsNullOrWhiteSpace(x.Abbreviation) && x.Abbreviation.Equals(erpAccount.StateProvince)))?.Id ?? 0;

                        if (address is null)
                        {
                            address = new Address();
                            address.FirstName = erpAccount.BillingName;
                            address.Email = erpAccount.Email;
                            address.Company = erpAccount.CompanyNo;
                            address.CountryId = countryId;
                            address.City = erpAccount.City;
                            address.County = erpAccount.Address3;
                            address.Address1 = erpAccount.Address1;
                            address.Address2 = erpAccount.Address2;
                            address.ZipPostalCode = erpAccount.ZipPostalCode;
                            address.StateProvinceId = stateProvinceId;
                            address.PhoneNumber = erpAccount.PhoneNumber;
                            address.FaxNumber = string.Empty;
                            address.CreatedOnUtc = DateTime.UtcNow;

                            await _addressService.InsertAddressAsync(address);
                        }
                        else
                        {
                            address.FirstName = erpAccount.BillingName;
                            address.Email = erpAccount.Email;
                            address.Company = erpAccount.CompanyNo;
                            address.CountryId = countryId;
                            address.City = erpAccount.City;
                            address.County = erpAccount.Address3;
                            address.Address1 = erpAccount.Address1;
                            address.Address2 = erpAccount.Address2;
                            address.ZipPostalCode = erpAccount.ZipPostalCode;
                            address.StateProvinceId = stateProvinceId;
                            address.PhoneNumber = erpAccount.PhoneNumber;
                            address.FaxNumber = string.Empty;

                            await _addressService.UpdateAddressAsync(address);
                        }

                        if (oldErpAccount is null)
                        {
                            oldErpAccount = new ErpAccount();
                            oldErpAccount.ErpSalesOrg = salesOrg;
                            oldErpAccount.ErpSalesOrgId = salesOrg.Id;

                            oldErpAccount.AccountNumber = erpAccount.AccountNumber;
                            oldErpAccount.AccountName = erpAccount.AccountName;
                            oldErpAccount.IsActive = erpAccount.IsActive;
                            oldErpAccount.VatNumber = erpAccount.VatNumber;
                            oldErpAccount.PreFilterFacets = erpAccount.PreFilterFacets;
                            oldErpAccount.PaymentTypeCode = erpAccount.PaymentTypeCode;
                            oldErpAccount.BillingAddressId = address.Id;
                            oldErpAccount.BillingSuburb = address.Address1;

                            oldErpAccount.AllowOverspend = erpAccount.AllowOverspend ? erpAccount.AllowOverspend : _b2BB2CFeaturesSettings.AllowOverspend;
                            oldErpAccount.AllowAccountsAddressEditOnCheckout = _b2BB2CFeaturesSettings.AllowAddressEditOnCheckoutForAll;
                            oldErpAccount.B2BPriceGroupCodeId = (await _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodedAsync(erpAccount.PriceGroupCode))?.Id ?? 0;

                            oldErpAccount.CreditLimitAvailable = erpAccount.CreditLimitAvailable ?? 0;
                            oldErpAccount.CreditLimit = erpAccount.CreditLimit ?? 0;
                            oldErpAccount.CurrentBalance = erpAccount.CurrentBalance ?? 0;

                            var hideStockValues = erpAccount.ErpAccountAttributes?.Exists(kvp =>
                                    HIDE_STOCK_VALUES.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)
                                    && bool.TryParse(kvp.Value, out var value)
                                    && value) ?? false;

                            oldErpAccount.OverrideStockDisplayFormatConfigSetting = false;
                            if (hideStockValues)
                            {
                                oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowInOrOutOfStockIndicators;
                            }
                            else
                            {
                                oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowStockQuantities;
                            }

                            oldErpAccount.ErpAccountStatusTypeId = (int)ErpAccountStatusType.Normal;
                            oldErpAccount.PercentageOfStockAllowed = erpAccount.PercentageOfStockAllowed ?? ALLOWED_STOCK_PERCENTAGE;

                            if (oldErpAccount.PercentageOfStockAllowed <= 0)
                            {
                                oldErpAccount.PercentageOfStockAllowed = ALLOWED_STOCK_PERCENTAGE;
                            }

                            oldErpAccount.IsDeleted = erpAccount.IsDeleted;

                            oldErpAccount.CreatedById = 1;
                            oldErpAccount.CreatedOnUtc = DateTime.UtcNow;
                            oldErpAccount.UpdatedById = 1;
                            oldErpAccount.UpdatedOnUtc = DateTime.UtcNow;
                            oldErpAccount.LastErpAccountSyncDate = DateTime.UtcNow;

                            erpAccountInsertList.Add(oldErpAccount);
                        }
                        else
                        {
                            oldErpAccount.AccountName = erpAccount.AccountName;
                            oldErpAccount.IsActive = erpAccount.IsActive;
                            oldErpAccount.VatNumber = erpAccount.VatNumber;
                            oldErpAccount.PreFilterFacets = erpAccount.PreFilterFacets;
                            oldErpAccount.PaymentTypeCode = erpAccount.PaymentTypeCode;
                            oldErpAccount.BillingAddressId = address.Id;
                            oldErpAccount.BillingSuburb = address.Address1;

                            oldErpAccount.AllowOverspend = erpAccount.AllowOverspend ? erpAccount.AllowOverspend : _b2BB2CFeaturesSettings.AllowOverspend;
                            oldErpAccount.AllowAccountsAddressEditOnCheckout = _b2BB2CFeaturesSettings.AllowAddressEditOnCheckoutForAll;
                            oldErpAccount.B2BPriceGroupCodeId = (await _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodedAsync(erpAccount.PriceGroupCode))?.Id ?? 0;

                            oldErpAccount.CreditLimitAvailable = erpAccount.CreditLimitAvailable ?? 0;
                            oldErpAccount.CreditLimit = erpAccount.CreditLimit ?? 0;
                            oldErpAccount.CurrentBalance = erpAccount.CurrentBalance ?? 0;

                            var hideStockValues = erpAccount.ErpAccountAttributes?.Exists(kvp =>
                                    HIDE_STOCK_VALUES.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)
                                    && bool.TryParse(kvp.Value, out var value)
                                    && value) ?? false;

                            oldErpAccount.OverrideStockDisplayFormatConfigSetting = false;
                            if (hideStockValues)
                            {
                                oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowInOrOutOfStockIndicators;
                            }
                            else
                            {
                                oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowStockQuantities;
                            }

                            oldErpAccount.ErpAccountStatusTypeId = (int)ErpAccountStatusType.Normal;
                            oldErpAccount.PercentageOfStockAllowed = erpAccount.PercentageOfStockAllowed ?? ALLOWED_STOCK_PERCENTAGE;

                            if (oldErpAccount.PercentageOfStockAllowed <= 0)
                            {
                                oldErpAccount.PercentageOfStockAllowed = ALLOWED_STOCK_PERCENTAGE;
                            }

                            oldErpAccount.IsDeleted = erpAccount.IsDeleted;

                            oldErpAccount.UpdatedById = 1;
                            oldErpAccount.UpdatedOnUtc = DateTime.UtcNow;
                            oldErpAccount.LastErpAccountSyncDate = DateTime.UtcNow;

                            erpAccountUpdateList.Add(oldErpAccount);
                        }

                        lastSyncedErpAccountNumber = oldErpAccount.AccountNumber;
                        totalSyncedSoFar++;
                    }

                    if (erpAccountInsertList.Count != 0)
                    {
                        await _erpAccountService.InsertErpAccountsAsync(erpAccountInsertList);
                        erpAccountInsertList.Clear();
                    }

                    if (erpAccountUpdateList.Count != 0)
                    {
                        await _erpAccountService.UpdateErpAccountsAsync(erpAccountUpdateList);
                        await _erpDataClearCacheService.ClearCacheOfEntities(erpAccountUpdateList);
                        erpAccountUpdateList.Clear();
                    }
                }

                if (!isError)
                {
                    //await _erpAccountService.InActiveAllOldAccount(syncStartTime);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                            ErpSyncLevel.Account,
                            $"Erp Accounts sync is successful for Sales Org: {salesOrg.Name}. The accounts which were updated before " +
                            $"{syncStartTime} are deactivated.");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                            ErpSyncLevel.Account,
                            $"Erp Accounts sync is partially or not successful for Sales Org: {salesOrg.Name}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                    ErpSyncLevel.Account,
                    (!string.IsNullOrWhiteSpace(lastSyncedErpAccountNumber) ?
                    $"The last synced Erp Account: {lastSyncedErpAccountNumber}, for Sales Org: {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar}");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                "Erp Account Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                ex.Message,
                ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                "Erp Account Sync ended.");

            return false;
        }
    }

    #endregion
}