using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpProductSettingModel : ODataIntegrationModel
{

    public ErpProductSettingModel()
    {
        ErpCategoryDataSettingsModel = new ErpCategoryDataSettingsModel();
        ErpGetRequestSettingsModel = new ErpGetRequestSettingsModel();
    }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Name")]
    public string? Name { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Sku")]
    public string? Sku { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ManufacturerPartNumber")]
    public string? ManufacturerPartNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ShortDescription")]
    public string? ShortDescription { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.FullDescription")]
    public string? FullDescription { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Height")]
    public string? Height { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Width")]
    public string? Width { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Length")]
    public string? Length { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Weight")]
    public string? Weight { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Price")]
    public string? Price { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.StockQuantity")]
    public string? StockQuantity { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.TaxCategoryId")]
    public string? TaxCategoryId { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductAttributes")]
    public string? ProductAttributes { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.TaxCategoryName")]
    public string? TaxCategoryName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Published")]
    public string? Published { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ManufacturerName")]
    public string? ManufacturerName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ManufacturerCode")]
    public string? ManufacturerCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.VendorCode")]
    public string? VendorCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.VendorName")]
    public string? VendorName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductTags")]
    public string? ProductTags { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.LastChangedDate")]
    public string? LastChangedDate { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.WarehouseNameOrCode")]
    public string? WarehouseNameOrCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.Gtin")]
    public string? Gtin { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpProductDataModel.Fields.ProductCost")]
    public string? ProductCost { get; set; }

    public ErpCategoryDataSettingsModel ErpCategoryDataSettingsModel { get; set; }

    public ErpGetRequestSettingsModel ErpGetRequestSettingsModel { get; set; }
}

