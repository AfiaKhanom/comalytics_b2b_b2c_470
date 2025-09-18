using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Services;
public class ErpNopMapperService : IErpNopMapperService
{
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;

    public ErpNopMapperService(B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ISpecificationAttributeService specificationAttributeService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings)
    {
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _specificationAttributeService = specificationAttributeService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
    }

    public async Task<IList<ErpAccountDataModel>> ErpAccountMapNop(IList<ErpAccountSqlResponseModel> erpAccountSqlResponses)
    {
        if (erpAccountSqlResponses == null)
            return new List<ErpAccountDataModel>();

        var erpAccounts = await erpAccountSqlResponses.Select(account => new ErpAccountDataModel
        {
            AccountNumber = account.AccountNumber ?? string.Empty,
            AccountName = account.AccountName ?? string.Empty,
            ErpSalesOrgCode = account.ErpSalesOrgCode ?? string.Empty,
            PaymentTypeCode = account.PaymentTypeCode ?? string.Empty,
            Address1 = account.Address1 ?? string.Empty,
            Address2 = account.Address2 ?? string.Empty,
            Address3 = account.Address3 ?? string.Empty,
            StateProvince = account.StateProvince ?? string.Empty,
            Country = account.Country ?? string.Empty,
            ZipPostalCode = account.ZipPostalCode ?? string.Empty,
            PhoneNumber = account.PhoneNumber ?? string.Empty,
            Email = account.Email ?? string.Empty,
            BillingName = account.BillingName ?? string.Empty,
            CompanyNo = account.CompanyNo ?? string.Empty,
            PreFilterFacets = account.PreFilterFacets ?? string.Empty,
            VatNumber = account.VatNumber ?? string.Empty,
            PriceGroupCode = account.PriceGroupCode ?? string.Empty,
            CreditLimit = account.CreditLimit ?? decimal.Zero,
            CreditLimitUsed = account.CreditLimitUsed ?? decimal.Zero,
            CreditLimitAvailable = account.CreditLimitAvailable ?? decimal.Zero,
            CurrentBalance = account.CurrentBalance ?? decimal.Zero,
            AllowOverspend = account.AllowOverspend,
            IsDeleted = account.IsDeleted,
            UpdatedOnUtc = account.UpdatedOnUtc,
            OverrideBackOrderingConfigSetting = account.OverrideBackOrderingConfigSetting,
            AllowAccountsBackOrdering = account.AllowAccountsBackOrdering,
            AllowAccountsAddressEditOnCheckout = account.AllowAccountsAddressEditOnCheckout,
            LoyaltyBalance = account.LoyaltyBalance ?? decimal.Zero,
            LoyaltyCardNumber = account.LoyaltyCardNumber ?? string.Empty,
            MinimumOrderValue = account.MinimumOrderValue,
            IsActive = account.IsActive,
            BillingSuburb = account.BillingSuburb ?? string.Empty,
            City = account.City ?? string.Empty
        }).ToListAsync();

        return erpAccounts;
    }

    public async Task<IList<ErpPlaceOrderDataModel>> ErpOrderMapNop(IList<ErpOrderSyncSqlResponseModel> erpOrderSqlResponses)
    {
        if (erpOrderSqlResponses == null || !erpOrderSqlResponses.Any())
            return new List<ErpPlaceOrderDataModel>();

        var groupedOrders = erpOrderSqlResponses.GroupBy(orderSync => orderSync.OrderNumber);
        var storeCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

        var erpOrders = groupedOrders.Select(group => new ErpPlaceOrderDataModel
        {
            AccountNumber = string.IsNullOrWhiteSpace(group.First().AccountNumber) ? string.Empty : group.First().AccountNumber,
            AccountName = string.IsNullOrWhiteSpace(group.First().AccountName) ? string.Empty : group.First().AccountName,
            Location = string.IsNullOrWhiteSpace(group.First().Location) ? string.Empty : group.First().Location,
            CustomOrderNumber = string.IsNullOrWhiteSpace(group.First().OrderNumber)
                       ? (string.IsNullOrWhiteSpace(group.First().EcomOrderNumber) ? string.Empty : group.First().EcomOrderNumber)
                       : group.First().OrderNumber,
            RepCode = string.Empty,
            AddressCode = string.Empty,
            ShippingAddress = new ErpAddressModel
            {
                Name = string.IsNullOrWhiteSpace(group.First().ShippingName) ? string.Empty : group.First().ShippingName,
                Email = string.IsNullOrWhiteSpace(group.First().ShippingEmail) ? string.Empty : group.First().ShippingEmail,
                Company = string.IsNullOrWhiteSpace(group.First().ShippingCompany) ? string.Empty : group.First().ShippingCompany,
                Address1 = string.IsNullOrWhiteSpace(group.First().ShippingAddress1) ? string.Empty : group.First().ShippingAddress1,
                Address2 = string.IsNullOrWhiteSpace(group.First().ShippingAddress2) ? string.Empty : group.First().ShippingAddress2,
                City = string.IsNullOrWhiteSpace(group.First().ShippingCity) ? string.Empty : group.First().ShippingCity,
                StateProvince = string.IsNullOrWhiteSpace(group.First().ShippingProvince) ? string.Empty : group.First().ShippingProvince,
                ZipPostalCode = string.IsNullOrWhiteSpace(group.First().ShippingPostalCode) ? string.Empty : group.First().ShippingPostalCode,
                Country = string.IsNullOrWhiteSpace(group.First().ShippingCountryCode) ? string.Empty : group.First().ShippingCountryCode,
                PhoneNumber = string.IsNullOrWhiteSpace(group.First().ShippingPhone) ? string.Empty : group.First().ShippingPhone
            },
            BillingAddress = new ErpAddressModel
            {
                Name = string.IsNullOrWhiteSpace(group.First().BillingName) ? string.Empty : group.First().BillingName,
                Email = string.IsNullOrWhiteSpace(group.First().BillingEmail) ? string.Empty : group.First().BillingEmail,
                Company = string.IsNullOrWhiteSpace(group.First().BillingCompany) ? string.Empty : group.First().BillingCompany,
                Address1 = string.IsNullOrWhiteSpace(group.First().BillingAddress1) ? string.Empty : group.First().BillingAddress1,
                Address2 = string.IsNullOrWhiteSpace(group.First().BillingAddress2) ? string.Empty : group.First().BillingAddress2,
                City = string.IsNullOrWhiteSpace(group.First().BillingCity) ? string.Empty : group.First().BillingCity,
                StateProvince = string.IsNullOrWhiteSpace(group.First().BillingProvince) ? string.Empty : group.First().BillingProvince,
                ZipPostalCode = string.IsNullOrWhiteSpace(group.First().BillingPostalCode) ? string.Empty : group.First().BillingPostalCode,
                Country = string.IsNullOrWhiteSpace(group.First().BillingCountryCode) ? string.Empty : group.First().BillingCountryCode,
                PhoneNumber = string.IsNullOrWhiteSpace(group.First().BillingPhone) ? string.Empty : group.First().BillingPhone
            },

            CustomerReference = group.First().CustomerReference ?? string.Empty,
            DeliveryInstruction = group.First().DeliveryInstruction ?? string.Empty,
            OrderCategory = string.Empty,
            DeliveryMethod = group.First().DeliveryMethod ?? string.Empty,

            CustomerName = group.First().CustomerName ?? "Comalytics Integration",
            CustomerFirstName = group.First().CustomerFirstName ?? "Comalytics",
            CustomerLastName = group.First().CustomerLastName ?? "Integration",
            CustomerPhoneNumber = group.First().CustomerPhoneNumber ?? "01234567890",
            CustomerMobileNumber = group.First().CustomerMobileNumber ?? "01234567890",
            CustomerEmail = group.First().CustomerEmail ?? "Integration@comalytics.com",

            VatNumber = string.Empty,
            OrderType = string.IsNullOrWhiteSpace(group.First().OrderType) ? string.Empty : group.First().OrderType,
            OrderTax = group.First().VAT,
            OrderSubtotalExclTax = group.First().TotalExcl,
            OrderSubtotalInclTax = group.First().TotalIncl,
            CustomerCurrencyCode = storeCurrency.CurrencyCode,
            OrderDate = group.First().OrderDate,
            DeliveryDate = group.First().DeliveryDate,
            DateRequired = group.First().DateRequired,

            // Aggregate all order items into a single list
            ErpPlaceOrderItemDatas = group.Select(orderSync => new ErpPlaceOrderItemDataModel
            {
                Sku = string.IsNullOrWhiteSpace(orderSync.Sku) ? string.Empty : orderSync.Sku,
                BatchCode = string.Empty,
                Description = string.IsNullOrWhiteSpace(orderSync.Description) ? string.Empty : orderSync.Description,
                Quantity = orderSync.Quantity,
                UnitOfMeasure = string.IsNullOrWhiteSpace(orderSync.OrderUom) ? string.Empty : orderSync.OrderUom,
                SpecialInstruction = string.IsNullOrWhiteSpace(orderSync.SpecInstruct) ? string.Empty : orderSync.SpecInstruct,
                UnitPriceExclTax = orderSync.UnitPriceExclTax,
                UnitPriceInclTax = orderSync.UnitPriceIncl,

                DiscountPercentage = decimal.Zero,
                PriceExclTax = orderSync.LineTotalExcl,
                PriceInclTax = orderSync.LineTotalIncl
            }).ToList()
        }).ToList();

        return erpOrders;
    }

    public async Task<IList<ErpPriceSpecialPricingDataModel>> ErpPriceSpecialPricingMapNop(IList<ErpPriceSpecialPricingSqlResponseModel> erpPriceSpecialPricingsResponseData)
    {
        if (erpPriceSpecialPricingsResponseData == null)
            return new List<ErpPriceSpecialPricingDataModel>();

        var specialPricing = await erpPriceSpecialPricingsResponseData.Select(price => new ErpPriceSpecialPricingDataModel
        {
            AccountNumber = price.AccountNumber ?? string.Empty,
            Branch = price.Branch ?? string.Empty,
            Sku = price.Sku ?? string.Empty,
            SpecialPrice = price.SpecialPrice ?? decimal.Zero,
            ListPrice = price.ListPrice ?? decimal.Zero,
            DiscountPercentage = price.DiscountPercentage ?? decimal.Zero
        }).ToListAsync();

        return specialPricing;
    }

    public async Task<IList<ErpProductDataModel>> ErpProductMapNop(IList<ErpProductSqlResponseModel> erpProductResponseData)
    {
        if (erpProductResponseData == null)
        {
            return new List<ErpProductDataModel>();
        }

        var preFilterSpecificAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(
            _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId);
        var uomSpecificAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(
            _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId);

        var erpProducts = (await Task.WhenAll(erpProductResponseData.Select(async product =>
        {
            return new ErpProductDataModel
            {
                Name = product.Name ?? string.Empty,
                Sku = product.Sku ?? string.Empty,
                ManufacturerPartNumber = product.ManufacturerPartNumber ?? string.Empty,
                ShortDescription = product.ShortDescription ?? string.Empty,
                FullDescription = product.FullDescription ?? string.Empty,
                Price = decimal.Zero,
                TaxCategoryName = product.TaxCategoryName ?? string.Empty,
                Published = SqlIntegrationDefaults.IsPublished(product.Published),
                VendorCode = product.VendorCode ?? string.Empty,
                VendorName = product.VendorName ?? string.Empty,
                Weight = decimal.TryParse(product.Weight, out var weight) ? weight : 0,
                Height = decimal.TryParse(product.Height, out var height) ? height : 0,
                Length = decimal.TryParse(product.Length, out var length) ? length : 0,
                Width = decimal.TryParse(product.Width, out var width) ? width : 0,
                ManufacturerCode = product.ManufacturerCode ?? string.Empty,
                ManufacturerName = product.ManufacturerName ?? string.Empty,
                LastChangedDate = product.LastChangedDate,
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
                    new (uomSpecificAttribute != null ? uomSpecificAttribute.Name : "UnitOfMeasure", product.UnitOfMeasure),
                    new (preFilterSpecificAttribute != null ? preFilterSpecificAttribute.Name : "PrefilterFacet", product.PrefilterFacet),
                    new ("Colour", product.Colour),
                    new ("Size", product.Size),
                    new ("Thickness", product.Thickness)
                },
                ProductTags = product.ProductTags ?? string.Empty,
                ProductCost = product.ProductCost,
                Gtin = product.Gtin ?? string.Empty
            };
        }))).ToList();

        return erpProducts;
    }

    public async Task<IList<ErpShipToAddressDataModel>> ErpShipToAddressMapNop(IList<ErpShipToAddressSqlResponseModel> erpShipToAddressResponseData)
    {
        if (erpShipToAddressResponseData == null)
            return new List<ErpShipToAddressDataModel>();

        var erpShipTo = (await Task.WhenAll(erpShipToAddressResponseData.Select(async shipto =>
        {
            return new ErpShipToAddressDataModel
            {
                AccountNumber = shipto.AccountNumber,
                ProvinceCode = shipto.ProvinceCode,
                ShipToCode = shipto.ShipToCode,
                ShipToName = shipto.ShipToName,
                Company = shipto.Company,
                Address1 = shipto.Address1,
                Address2 = shipto.Address2,
                City = shipto.City,
                StateProvince = shipto.Province,
                Suburb = shipto.Suburb,
                Country = shipto.Country,
                ZipPostalCode = shipto.ZipPostalCode,
                PhoneNumber = shipto.PhoneNumber,
                FaxNumber = string.Empty,
                CustomAttributes = string.Empty,
                DeliveryNotes = shipto.DeliveryNotes,
                EmailAddress = shipto.EmailAddress,
                RepNumber = shipto.RepNumber,
                RepFullName = shipto.RepName,
                RepPhoneNumber = shipto.RepPhoneNumber,
                RepEmail = shipto.RepEmailAddress,
                SalesOrgCode = shipto.SalesOrgCode
            };
        }))).ToList();

        return erpShipTo;
    }

    public async Task<IList<ErpStockDataModel>> ErpStockMapNop(IList<ErpStockSqlResponseModel> erpStockResponseData)
    {
        if (erpStockResponseData == null)
            return new List<ErpStockDataModel>();

        var erpStock = (await Task.WhenAll(erpStockResponseData.Select(async stock =>
        {
            return new ErpStockDataModel
            {
                SalesOrgCode = stock.SalesOrgCode ?? string.Empty,
                WarehouseNameOrCode = stock.WarehouseNameOrCode ?? string.Empty,
                Sku = stock.Sku ?? string.Empty,
                QuantityOnHand = stock.QuantityOnHand ?? 0,
                LastChangedDate = stock.LastChangedDate ?? null
            };
        }))).ToList();

        return erpStock;
    }

    public async Task<IList<ErpInvoiceDataModel>> ErpInvoiceMapNop(IList<ErpInvoiceSqlResponseModel> erpInvoicesResponseData)
    {
        if (erpInvoicesResponseData == null)
            return new List<ErpInvoiceDataModel>();

        var erpInvoice = (await Task.WhenAll(erpInvoicesResponseData.Select(async invoice =>
        {
            return new ErpInvoiceDataModel
            {
                PostingDateUtc = invoice.PostingDateUtc,
                ErpDocumentNumber = invoice.ErpDocumentNumber,
                Description = invoice.Description,
                AmountExclVat = invoice.AmountExclVat,
                AmountInclVat = invoice.AmountInclVat,
                CurrencyCode = invoice.CurrencyCode,
                DocumentType = invoice.DocumentType,
                DocumentDisplayName = invoice.DocumentDisplayName,
                PODSignedById = invoice.PODSignedById,
                PODSignedOnUtc = invoice.PODSignedOnUtc,
                DueDateUtc = invoice.DueDateUtc,
                RelatedDocumentNo = invoice.RelatedDocumentNo,
                ShipmentDateUtc = invoice.ShipmentDateUtc,
                DocumentDateUtc = invoice.DocumentDateUtc,
                ErpOrderNumber = invoice.ErpOrderNumber,
            };
        }))).ToList();

        return erpInvoice;
    }

    public async Task<IList<ErpPriceGroupPricingDataModel>> ErpPriceGroupPricingMapNopAsync(IList<ErpPriceGroupPricingSqlResponseModel> erpPriceGroupPricingsResponseData)
    {
        if (erpPriceGroupPricingsResponseData == null)
            return await Task.FromResult(new List<ErpPriceGroupPricingDataModel>());

        return await erpPriceGroupPricingsResponseData.Select(price => new ErpPriceGroupPricingDataModel
        {
            GroupPriceCode = price.GroupPriceCode ?? string.Empty,
            Sku = price.Sku ?? string.Empty,
            Price = price.Price ?? decimal.Zero,
            GroupPrices = new Dictionary<string, decimal?>(),
        }).ToListAsync();
    }
}
