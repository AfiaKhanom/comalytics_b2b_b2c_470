using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;
public class ErpNopMapperService : IErpNopMapperService
{
    public async Task<IList<ErpAccountDataModel>> ErpAccountMapNop(IList<ErpAccountSysproResponseModel> erpAccountSysproResponses)
    {
        if (erpAccountSysproResponses == null)
            return new List<ErpAccountDataModel>();

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
            CreditLimit = account.CreditLimit.HasValue ? account.CreditLimit.Value : decimal.Zero,
            CreditLimitUsed = account.CurrentBalance ?? decimal.Zero,
            CreditLimitAvailable = account.AvailableCredit ?? decimal.Zero,
            CurrentBalance = account.CurrentBalance ?? decimal.Zero,
            AllowOverspend = account.AllowOverspend,
            IsDeleted = account.isDeleted,
            UpdatedOnUtc = account.LastChanged,
            OverrideBackOrderingConfigSetting = account.AllowbackOrdering,
            AllowAccountsBackOrdering = account.AllowbackOrdering,
            AllowAccountsAddressEditOnCheckout = account.AllowAddressChangeOnCheckout

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
                Price = decimal.Zero,
                TaxCategoryName = product.TaxCategory ?? string.Empty,
                Published = product.Published,
                VendorCode = product.VendorCode ?? string.Empty,
                VendorName = product.VendorName ?? string.Empty,
                Weight = decimal.TryParse(product.Weight, out var weight) ? weight : 0,
                Height = decimal.TryParse(product.Weight, out var height) ? height : 0,
                Length = decimal.TryParse(product.Weight, out var length) ? length : 0,
                Width = decimal.TryParse(product.Weight, out var width) ? width : 0,
                ManufacturerCode = product.ManufacturerCode ?? string.Empty,
                ManufacturerName = product.ManufacturerName ?? string.Empty,
                LastChangedDate = product.LastChangeDate,
                ProductCategories = new List<ErpCategoryDataModel>()
                    {
                        new ()
                        {
                            CategoryName = product.CategoryName1 ?? string.Empty
                        },
                        new ()
                        {
                            CategoryName = product.CategoryName2 ?? string.Empty
                        },
                        new ()
                        {
                            CategoryName = product.CategoryName3 ?? string.Empty
                        }
                    },
                ProductAttributes = new List<KeyValuePair<string, string>>()
                {
                    new ("UnitOfMeasure", product.UnitOfMeasure),
                    new ("PrefilterFacet", product.PrefilterFacet),
                    new ("Colour", product.Colour),
                    new ("Size", product.Size),
                    new ("Thickness", product.Thickness)
                }
            };
        }))).ToList();

        return erpProducts;
    }
}
