using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SalesRepUser;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class SalesRepUserModelFactory : ISalesRepUserModelFactory
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IErpSalesRepService _erpSalesRepService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IAddressService _addressService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IB2BFeaturesCommonHelper _b2BFeaturesCommonHelper;

    #endregion

    #region Ctor

    public SalesRepUserModelFactory(ICustomerService customerService,
        IErpAccountService erpAccountService,
        IDateTimeHelper dateTimeHelper,
        IErpSalesRepService erpSalesRepService,
        IErpSalesOrgService erpSalesOrgService,
        IAddressService addressService,
        IAddressModelFactory addressModelFactory,
        ILocalizationService localizationService,
        IB2BFeaturesCommonHelper b2BFeaturesCommonHelper)
    {
        _customerService = customerService;
        _erpAccountService = erpAccountService;
        _dateTimeHelper = dateTimeHelper;
        _erpSalesRepService = erpSalesRepService;
        _erpSalesOrgService = erpSalesOrgService;
        _addressService = addressService;
        _addressModelFactory = addressModelFactory;
        _localizationService = localizationService;
        _b2BFeaturesCommonHelper = b2BFeaturesCommonHelper;
    }

    #endregion

    #region Methods

    public async Task<SalesRepUserSearchModel> PrepareSalesRepUserSearchModelAsync(SalesRepUserSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgAsync();
        searchModel.AvailableSalesOrgs = salesOrgs.Select(x => new SelectListItem
        {
            Text = $"{x.Name} ({x.Code})",
            Value = $"{x.Id}"
        }).ToList();
        searchModel.AvailableSalesOrgs.Insert(0, new SelectListItem 
        { 
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowAll"),
            Value = "0" 
        });

        searchModel.ShowInActiveOption = await _b2BFeaturesCommonHelper.PrepareActiveFilterOptionsAsync();

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<SalesRepUserListModel> PrepareSalesRepUserListModelForSalesRep(SalesRepUserSearchModel searchModel, ErpSalesRep erpSalesRep)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpNopUsers = await _erpSalesRepService.GetAllSalesRepUsersAsync(
            salesRepId: erpSalesRep.Id,
            salesRepCustomerId: erpSalesRep.NopCustomerId,
            salesRepTypeId: erpSalesRep.SalesRepTypeId,
            erpAccontNo: searchModel.SearchERPAccountNumber,
            accountName: searchModel.SearchERPAccountName,
            email: searchModel.SearchCustomerEmail,
            salesOrgId: searchModel.SearchSalesOrgId,
            isActive: searchModel.SearchActiveId == 0 ? null : searchModel.SearchActiveId == (int)ActiveFilterType.ShowOnlyActive,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var salesOrgs = await _erpSalesOrgService.GetErpSalesOrgsAsync();

        var model = await new SalesRepUserListModel().PrepareToGridAsync(searchModel, erpNopUsers, () =>
        {
            return erpNopUsers.SelectAwait(async user =>
            {
                var customer = await _customerService.GetCustomerByIdAsync(user.NopCustomerId);
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(user.ErpAccountId);

                var userModel = new SalesRepUserModel
                {
                    Id = user.Id,
                    NopCustomerId = user.NopCustomerId,
                    CustomerFullName = await _customerService.GetCustomerFullNameAsync(customer),
                    CustomerEmail = customer.Email,
                    ErpShipToAddressId = user.ErpShipToAddressId,
                    CreatedOnUtc = user.CreatedOnUtc,
                    IsActive = user.IsActive,
                    ErpUserType = $"{(ErpUserType)user.ErpUserTypeId}",
                };

                if (erpAccount != null)
                {
                    userModel.ErpAccountId = user.ErpAccountId;
                    userModel.ErpAccountNumber = erpAccount.AccountNumber;
                    userModel.ErpAccountName = erpAccount.AccountName;

                    var salesOrg = salesOrgs.FirstOrDefault(x => x.Id == erpAccount.ErpSalesOrgId);
                    if (salesOrg != null)
                    {
                        userModel.ErpSalesOrgName = $"{salesOrg.Name} ({salesOrg.Code})";
                    }
                }

                return userModel;
            });
        });

        return model;
    }

    public async Task<ErpAccountListModel> PrepareSalesRepErpUserListModelForSalesRep(ErpAccountSearchModel searchModel, ErpSalesRep erpSalesRep)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpAccountIdMaps = 
            (await _erpAccountService.GetAllErpAccountsBySalesRepIdAsync(salesRepId: Convert.ToInt32(searchModel.ErpAccountId)))
            .ToPagedList(searchModel);

        var model = await new ErpAccountListModel().PrepareToGridAsync(searchModel, erpAccountIdMaps, () =>
        {
            return erpAccountIdMaps.SelectAwait(async erpIdMap =>
            {
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpIdMap.ErpAccountId);
                if (erpAccount == null)
                    return null;

                var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0);
                var addressModel = new AddressModel();
                if (address != null)
                    addressModel = address.ToModel(addressModel);
                await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

                var erpAccountModel = new ErpAccountModel
                {
                    Id = erpAccount.Id,
                    AccountNumber = erpAccount.AccountNumber,
                    AccountName = erpAccount.AccountName,
                    VatNumber = erpAccount.VatNumber,
                    CurrentBalance = erpAccount.CurrentBalance,
                    ErpSalesOrgId = erpAccount.ErpSalesOrgId,
                    BillingAddressId = erpAccount.BillingAddressId,
                    BillingAddress = addressModel,
                    BillingSuburb = erpAccount.BillingSuburb,
                    CreditLimit = erpAccount.CreditLimit,
                    CreditLimitAvailable = erpAccount.CreditLimitAvailable,
                    LastPaymentAmount = erpAccount.LastPaymentAmount,
                    LastPaymentDate = erpAccount.LastPaymentDate,
                    AllowOverspend = erpAccount.AllowOverspend,
                    PreFilterFacets = erpAccount.PreFilterFacets,
                    PaymentTypeCode = erpAccount.PaymentTypeCode,
                    OverrideAddressEditOnCheckoutConfigSetting = erpAccount.OverrideAddressEditOnCheckoutConfigSetting,
                    OverrideBackOrderingConfigSetting = erpAccount.OverrideBackOrderingConfigSetting,
                    AllowAccountsAddressEditOnCheckout = erpAccount.AllowAccountsAddressEditOnCheckout,
                    AllowAccountsBackOrdering = erpAccount.AllowAccountsBackOrdering,
                    OverrideStockDisplayFormatConfigSetting = erpAccount.OverrideStockDisplayFormatConfigSetting,
                    ErpAccountStatusTypeId = erpAccount.ErpAccountStatusTypeId,
                    ErpAccountStatusType = ((ErpAccountStatusType)erpAccount.ErpAccountStatusTypeId).ToString(),
                    LastErpAccountSyncDate = erpAccount.LastErpAccountSyncDate,
                    B2BPriceGroupCodeId = erpAccount.B2BPriceGroupCodeId,
                    TotalSavingsForthisYear = erpAccount.TotalSavingsForthisYear ?? 0,
                    TotalSavingsForAllTime = erpAccount.TotalSavingsForAllTime ?? 0,
                    TotalSavingsForAllTimeUpdatedOnUtc = erpAccount.TotalSavingsForAllTimeUpdatedOnUtc,
                    TotalSavingsForthisYearUpdatedOnUtc = erpAccount.TotalSavingsForthisYearUpdatedOnUtc,
                    LastTimeOrderSyncOnUtc = erpAccount.LastTimeOrderSyncOnUtc,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.CreatedOnUtc, DateTimeKind.Utc),
                    UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.UpdatedOnUtc, DateTimeKind.Utc),
                    IsActive = erpAccount.IsActive
                };

                var erpAccountSalesOrgInfo = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                if (erpAccountSalesOrgInfo != null)
                {
                    erpAccountModel.ErpSalesOrgName = $"{erpAccountSalesOrgInfo.Name} - ({erpAccountSalesOrgInfo.Code})";
                }
                return erpAccountModel;
            });
        });

        return model;
    }

    #endregion
}