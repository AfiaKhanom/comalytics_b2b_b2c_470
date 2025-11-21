using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
public class ErpProductSettings : ISettings
{
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public string? ManufacturerPartNumber { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? Height { get; set; }
    public string? Width { get; set; }
    public string? Length { get; set; }
    public string? Weight { get; set; }
    public string? Price { get; set; }
    public string? StockQuantity { get; set; }
    public string? TaxCategoryId { get; set; }
    public string? TaxCategoryName { get; set; }
    public string? Published { get; set; }
    public string? ManufacturerName { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }
    public string? ProductTags { get; set; }
    public string? LastChangedDate { get; set; }
    public string? WarehouseNameOrCode { get; set; }
    public string? Gtin { get; set; }
    public string? ProductCost { get; set; }
    public string? ProductAttributes { get; set; }
}

