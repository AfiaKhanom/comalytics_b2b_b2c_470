using System;
using System.Collections.Generic;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpProductDataModel
{
    public string Name { get; set; }
    public string Sku { get; set; }
    public string ManufacturerPartNumber { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Length { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Price { get; set; }
    public decimal? StockQuantity { get; set; }

    // If we are mapping multiple categories to a product, and each category might have multiple child
    // In this process, the Categories will contain full category paths, like "A >> B >> C >> D"
    public bool IsUsingCategoryPathMapping { get; set; }
    public IEnumerable<ErpCategoryDataModel> Categories { get; set; } = new List<ErpCategoryDataModel>();
    public List<KeyValuePair<string, string>> ProductAttributes { get; set; } = new List<KeyValuePair<string, string>>();
    public int TaxCategoryId { get; set; }
    public string TaxCategoryName { get; set; }
    public bool Published { get; set; }
    public string ManufacturerName { get; set; }
    public string ManufacturerCode { get; set; }
    public string VendorCode { get; set; }
    public string VendorName { get; set; }
    public string ProductTags { get; set; }
    public DateTime? LastChangedDate { get; set; }
    public string WarehouseNameOrCode { get; set; }
    public string Gtin { get; set; }
    public decimal? ProductCost { get; set; }
}
