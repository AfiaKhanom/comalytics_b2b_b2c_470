namespace NopStation.Plugin.Misc.ErpSqlIntegration.Settings;

public class ErpPlaceOrderItemSettings : AdditionalHardcodedValueSettings
{
    public string? Sku { get; set; }
    public string? BatchCode { get; set; }
    public string? Description { get; set; }
    public string? Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? SpecialInstruction { get; set; }
    public string? UnitPriceExclTax { get; set; }
    public string? UnitPriceInclTax { get; set; }
    public string? DiscountPercentage { get; set; }
    public string? DiscountAmountInclTax { get; set; }
    public string? DiscountAmountExclTax { get; set; }
    public string? PriceExclTax { get; set; }
    public string? PriceInclTax { get; set; }
    public string? ErpOrderPayloadLinesKey { get; set; }
    public string? WarehouseCode { get; set; }
}