using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
public record ErpOrderItemDataSettingsModel : AdditionalHardcodedValueModel
{
    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.Sku")]
    public string? Sku { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.BatchCode")]
    public string? BatchCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.Description")]
    public string? Description { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.Quantity")]
    public string? Quantity { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.UnitOfMeasure")]
    public string? UnitOfMeasure { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.SpecialInstruction")]
    public string? SpecialInstruction { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.UnitPriceExclTax")]
    public string? UnitPriceExclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.UnitPriceInclTax")]
    public string? UnitPriceInclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.DiscountPercentage")]
    public string? DiscountPercentage { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.DiscountAmountInclTax")]
    public string? DiscountAmountInclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.DiscountAmountExclTax")]
    public string? DiscountAmountExclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.PriceExclTax")]
    public string? PriceExclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderItemDataSettings.Fields.PriceInclTax")]
    public string? PriceInclTax { get; set; }


    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpGetRequestSettingsModel.Fields.ErpOrderPayloadLinesKey")]
    public string? ErpOrderPayloadLinesKey { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpGetRequestSettingsModel.Fields.WarehouseCode")]
    public string? WarehouseCode { get; set; }
}

