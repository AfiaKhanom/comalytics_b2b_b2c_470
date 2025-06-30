using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Areas.Admin.Models;
public record ErpOrderSettingsModel : AdditionalHardcodedValueModel
{
    public ErpOrderSettingsModel()
    {
        ErpOrderItemDataSettingsModel = new ErpOrderItemDataSettingsModel();
    }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.AccountNumber")]
    public string? AccountNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.Location")]
    public string? Location { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.CustomOrderNumber")]
    public string? CustomOrderNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.RepCode")]
    public string? RepCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.AddressCode")]
    public string? AddressCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.Notes")]
    public string? Notes { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.CustomerReference")]
    public string? CustomerReference { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DeliveryInstruction")]
    public string? DeliveryInstruction { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DeliveryMethod")]
    public string? DeliveryMethod { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.CustomerEmail")]
    public string? CustomerEmail { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderType")]
    public string? OrderType { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderTax")]
    public string? OrderTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderSubtotalExclTax")]
    public string? OrderSubtotalExclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderSubtotalInclTax")]
    public string? OrderSubtotalInclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.OrderDate")]
    public string? OrderDate { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DeliveryDate")]
    public string? DeliveryDate { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.DateRequired")]
    public string? DateRequired { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.ShippingAmount")]
    public string? ShippingAmount { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.Misc.ErpSqlIntegration.ErpOrderSettingsModel.Fields.ErpOrderPayloadRootKey")]
    public string? ErpOrderPayloadRootKey { get; set; }

    public ErpOrderItemDataSettingsModel ErpOrderItemDataSettingsModel { get; set; }
}

