using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public class ErpNopMapperService : IErpNopMapperService
{
    public Task<IList<ErpAccountDataModel>> ErpAccountMapNop(IList<ErpAccountSysproResponseModel> erpAccountSysproResponses)
    {
        if (erpAccountSysproResponses == null)
            return Task.FromResult<IList<ErpAccountDataModel>>(new List<ErpAccountDataModel>());

        var erpAccounts = erpAccountSysproResponses.Select(account => new ErpAccountDataModel
        {
            AccountNumber = account.Customer ?? string.Empty,
            AccountName = account.Name ?? string.Empty,
            ErpSalesOrgCode = account.Company ?? string.Empty,
            PaymentTypeCode = account.PaymentTermsCode ?? string.Empty,
            Address1 = account.BillingAddress1 ?? string.Empty,
            Address2 = account.BillingAddress2 ?? string.Empty,
            Address3 = account.BillingAddress1 ?? string.Empty,
            StateProvince = account.BillingProvince ?? string.Empty,
            Country = account.BillingCountry ?? string.Empty,
            ZipPostalCode = account.BillingPostalCode ?? string.Empty,
            PhoneNumber = account.BillingPhoneNumber ?? string.Empty,
            Email = account.BillingEmail ?? string.Empty,
            BillingName = account.BillingName ?? string.Empty,
            CompanyNo = string.Empty,
            PreFilterFacets = string.Empty,
            VatNumber = account.VatNumber ?? string.Empty,
            PriceGroupCode = account.PriceGroupCode ?? string.Empty,
             = account.CreditLimit.HasValue ? account.CreditLimit.Value : decimal.Zero,
            CreditLimitUsed = account.CurrentBalance ?? decimal.Zero,
            CreditLimitAvailable = account.AvailableCredit ?? decimal.Zero,
            CurrentBalance = account.CurrentBalance ?? decimal.Zero
        }).ToList();

        return Task.FromResult<IList<ErpAccountDataModel>>(erpAccounts);
    }
}
