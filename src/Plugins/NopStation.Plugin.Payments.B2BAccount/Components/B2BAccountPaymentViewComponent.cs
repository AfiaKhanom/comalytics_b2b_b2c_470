using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpAccountCreditSyncFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Payments.B2BAccount.Models;

namespace NopStation.Plugin.Payments.B2BAccount.Components;

public class B2BAccountPaymentViewComponent : NopViewComponent
{
    #region Fields

    private readonly IErpNopUserService _erpNopUserService;
    private readonly IWorkContext _workContext;
    private readonly IErpAccountService _erpAccountService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpAccountCreditSyncFunctionality _erpAccountCreditSyncFunctionality;

    #endregion

    #region Ctor

    public B2BAccountPaymentViewComponent(IErpNopUserService erpNopUserService,
        IWorkContext workContext,
        IErpAccountService erpAccountService,
        IPriceFormatter priceFormatter,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpLogsService erpLogsService,
        IErpSalesOrgService erpSalesOrgService,
        IErpAccountCreditSyncFunctionality erpAccountCreditSyncFunctionality)
    {
        _erpNopUserService = erpNopUserService;
        _workContext = workContext;
        _erpAccountService = erpAccountService;
        _priceFormatter = priceFormatter;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpLogsService = erpLogsService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpAccountCreditSyncFunctionality = erpAccountCreditSyncFunctionality;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new B2BAccountPaymentInfoModel();

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return Content(string.Empty);

        var nopErpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
        if (nopErpUser == null || nopErpUser.ErpUserType != ErpUserType.B2BUser)
            return Content(string.Empty);

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(nopErpUser.ErpAccountId);
        if (erpAccount == null)
            return Content(string.Empty);

        await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(erpAccount, customer);

        model.CreditLimitAvailableStr = await _priceFormatter.FormatPriceAsync(erpAccount.CreditLimitAvailable, true, false);
        model.CreditLimitStr = await _priceFormatter.FormatPriceAsync(erpAccount.CreditLimit, true, false);
        model.CurrentBalanceStr = await _priceFormatter.FormatPriceAsync(erpAccount.CurrentBalance, true, false);
        model.ErpAccountId = erpAccount.Id;

        return View("~/Plugins/NopStation.Plugin.Payments.B2BAccount/Views/Shared/Components/B2BAccountPayment/Default.cshtml", model);
    }

    #endregion
}
