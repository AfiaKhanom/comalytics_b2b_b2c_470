using System.Collections.Generic;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpPriceGroupPricingDataModel
{
    public string SalesOrgCode { get; set; }
    public string GroupPriceCode { get; set; }
    public string Sku { get; set; }
    public decimal? Price { get; set; }
    public List<ErpGroupPriceListDataModel> GroupPrices { get; set; }
}
