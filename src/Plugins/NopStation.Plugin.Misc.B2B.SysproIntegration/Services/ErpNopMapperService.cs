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

    public async Task<IList<ErpProductDataModel>> ErpProductMapNop(IList<ErpProductSysproResponseModel> erpProductResponseData)
    {
        var erpProducts = (await Task.WhenAll(erpProductResponseData.Select(async product =>
        {
            return new ErpProductDataModel
            {
                Name = product.ProductName ?? string.Empty,
                ItemNo = product.StockCode ?? string.Empty,
                MasterCode = product.ManufacturerPartNumber ?? string.Empty,
                Description = product.Description ?? string.Empty,
                IsSpecial = false,
                FullDescription = product.Description ?? string.Empty,
                SellingPriceA = product.SellPrice1,
                UnitOfMeasure = string.Empty,
                VatRate = product.VatRate ?? string.Empty,
                Active = string.Empty,
                VendorName = string.Empty,
                Brand = product.BrandName,
                BrandDesc = product.AlternativeDescription ?? string.Empty,
                Categories = new List<ErpProductCategory>()
                    {
                        new ()
                        {
                            CategoryCode =  string.Empty,
                            CategoryName = product.DepartmentName ?? string.Empty
                        },
                        new ()
                        {
                            CategoryCode =  string.Empty,
                            CategoryName = product.GroupName ?? string.Empty
                        },
                        new ()
                        {
                            CategoryCode =  string.Empty,
                            CategoryName = product.CategoryName ?? string.Empty
                        }
                    },
                Attributes = new List<KeyValuePair<string, string>>()
                    {
                        new (nameof(ErpStockRecordModel.Colour), product.Colour),
                        new (nameof(ErpStockRecordModel.Size), product.Size)
                    }
            };
        }))).ToList();

        return erpproduct;
    }
}
