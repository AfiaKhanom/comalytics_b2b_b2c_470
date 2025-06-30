namespace NopStation.Plugin.Misc.ErpSqlIntegration.Models;
public class ErpProductSqlResponseModel
{
    public string Sku { get; set; }
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public string CategoryName1 { get; set; }
    public string CategoryName2 { get; set; }
    public string CategoryName3 { get; set; }
    public string ManufacturerCode { get; set; }
    public string ManufacturerName { get; set; }
    public string Published { get; set; }
    public string ProductTags { get; set; }
    public string Gtin { get; set; }
    public string ManufacturerPartNumber { get; set; }
    public string VendorCode { get; set; }
    public string VendorName { get; set; }
    public string TaxCategoryName { get; set; }
    public string Weight { get; set; }
    public string Length { get; set; }
    public string Width { get; set; }
    public string Height { get; set; }
    public string PrefilterFacet { get; set; }
    public string UnitOfMeasure { get; set; }
    public string Colour { get; set; }
    public string Size { get; set; }
    public string Thickness { get; set; }
    public DateTime? LastChangedDate { get; set; }
    public decimal? ProductCost { get; set; }
}
