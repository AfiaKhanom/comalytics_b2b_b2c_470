namespace NopStation.Plugin.Misc.ErpSqlIntegration.Models;
public class ErpPriceSpecialPricingSqlResponseModel
{
    public string Branch { get; set; }
    public string Sku { get; set; }
    public string AccountNumber { get; set; }
    public decimal? SpecialPrice { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
}
