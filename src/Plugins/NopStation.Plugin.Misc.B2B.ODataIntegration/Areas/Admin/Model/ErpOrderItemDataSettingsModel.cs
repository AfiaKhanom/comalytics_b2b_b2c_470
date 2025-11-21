using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpOrderItemDataSettingsModel : AdditionalHardcodedValueModel
{

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.Sku")]
    public string? Sku { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.BatchCode")]
    public string? BatchCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.Description")]
    public string? Description { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.Quantity")]
    public string? Quantity { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.UnitOfMeasure")]
    public string? UnitOfMeasure { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.SpecialInstruction")]
    public string? SpecialInstruction { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.UnitPriceExclTax")]
    public string? UnitPriceExclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.UnitPriceInclTax")]
    public string? UnitPriceInclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.DiscountPercentage")]
    public string? DiscountPercentage { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.DiscountAmountInclTax")]
    public string? DiscountAmountInclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.DiscountAmountExclTax")]
    public string? DiscountAmountExclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.PriceExclTax")]
    public string? PriceExclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderItemDataSettings.Fields.PriceInclTax")]
    public string? PriceInclTax { get; set; }
}

