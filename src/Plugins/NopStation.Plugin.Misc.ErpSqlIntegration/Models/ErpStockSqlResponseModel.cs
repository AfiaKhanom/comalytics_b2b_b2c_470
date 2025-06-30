namespace NopStation.Plugin.Misc.ErpSqlIntegration.Models;
public class ErpStockSqlResponseModel
{
    public string SalesOrgCode { get; set; }
    public string Sku { get; set; }
    public string WarehouseNameOrCode { get; set; }
    public decimal? QuantityOnHand { get; set; }
    public DateTime? LastChangedDate { get; set; }
}
