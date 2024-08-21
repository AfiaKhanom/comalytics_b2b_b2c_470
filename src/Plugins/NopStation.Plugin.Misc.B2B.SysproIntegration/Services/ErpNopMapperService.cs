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
            Branch = account.Company ?? string.Empty,
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
            CreditLimitAvailableStr = account.CreditLimit.HasValue ? account.CreditLimit.Value.ToString() : "0",
            CreditLimitUsed = account.CurrentBalance ?? decimal.Zero,
            CreditLimitAvailable = account.AvailableCredit ?? decimal.Zero,
            CurrentBalance = account.CurrentBalance ?? decimal.Zero
        }).ToList();

        return erpAccounts;
    }

    public async Task<IList<ErpPriceSpecialPricingDataModel>> ErpPriceSpecialPricingMapNop(List<ErpPriceSpecialPricingSysproResponseModel> erpPriceSpecialPricingsResponseData)
    {
        if (erpPriceSpecialPricingsResponseData == null)
            return new List<ErpPriceSpecialPricingDataModel>();

        var specialPricing = erpPriceSpecialPricingsResponseData.Select(price => new ErpPriceSpecialPricingDataModel
        {
            AccountNumber = price.Customer ?? string.Empty,
            Branch = price.Company ?? string.Empty,
            Sku = price.StockCode ?? string.Empty,
            SpecialPrice = price.Price ?? decimal.Zero
        }).ToList();

        return specialPricing;
    }

    public async Task<IList<ErpProductDataModel>> ErpProductMapNop(IList<ErpProductSysproResponseModel> erpProductResponseData)
    {
        var erpProducts = (await Task.WhenAll(erpProductResponseData.Select(async product =>
        {
            return new ErpProductDataModel
            {
                Name = product.ProductName ?? string.Empty,
                Sku = product.StockCode ?? string.Empty,
                ManufacturerPartNumber = product.ManufacturerPartNumber ?? string.Empty,
                ShortDescription = product.ShortDescription ?? string.Empty,
                FullDescription = product.LongDescription ?? string.Empty,
                Price = decimal.zer,
                UnitOfMeasure = string.Empty,
                tax = product.VatRate ?? string.Empty,
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
