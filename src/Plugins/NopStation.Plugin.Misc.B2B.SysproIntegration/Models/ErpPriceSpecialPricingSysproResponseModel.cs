namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Models;
public class ErpPriceSpecialPricingSysproResponseModel
{
    public string Company { get; set; }
    public string StockCode { get; set; }
    public string Customer { get; set; }
    public decimal? Price { get; set; }
    public string PriceCode { get; set; }
    public string Uom { get; set; }
}
