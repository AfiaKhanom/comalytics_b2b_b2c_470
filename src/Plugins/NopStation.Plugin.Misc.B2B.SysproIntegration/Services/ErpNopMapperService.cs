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
            AccNo = account.Customer ?? string.Empty,
            Name = account.Name ?? string.Empty,
            Branch = account.Company ?? string.Empty,
            Notes = account.PaymentTermsDescription ?? string.Empty,
            Address1 = account.BillingAddress1 ?? string.Empty,
            Address2 = account.BillingAddress2 ?? string.Empty,
            Address3 = account.BillingAddress1 ?? string.Empty,
            Province = account.BillingProvince ?? string.Empty,
            Country = account.BillingCountry ?? string.Empty,
            PostalCode = account.BillingPostalCode ?? string.Empty,
            TelNo = account.BillingPhoneNumber ?? string.Empty,
            EMail = account.BillingEmail ?? string.Empty,
            EMail1 = account.BillingEmail ?? string.Empty,
            DelName = account.BillingName ?? string.Empty,
            CompanyNo = string.Empty,
            PrefilterFacets = string.Empty,
            VatNumber = account.VatNumber ?? string.Empty,
            PriceGroupCode = account.PriceGroupCode ?? string.Empty,
            CreditLimit = account.CreditLimit.HasValue ? account.CreditLimit.Value : decimal.Zero,
            CreditLimitUsed = account.CurrentBalance,
            CreditLimitAvailable = account.AvailableCredit,
            Balance = account.CurrentBalance
        }).ToList();

        return Task.FromResult<IList<ErpAccountDataModel>>(erpAccounts);
    }
}
