using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpOrderSettingsModel : AdditionalHardcodedValueModel
{

    public ErpOrderSettingsModel()
    {
        ErpOrderItemDataSettingsModel = new ErpOrderItemDataSettingsModel();
        ErpShippingAddressSettingsModel = new ErpShippingAddressModel();
        ErpBillingAddressSettingsModel = new ErpBillingAddressModel();
        ErpGetRequestSettingsModel = new ErpGetRequestSettingsModel();
    }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.AccountNumber")]
    public string? AccountNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.SalesOrg")]
    public string? Location { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomOrderNumber")]
    public string? CustomOrderNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.RepCode")]
    public string? RepCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.AddressCode")]
    public string? AddressCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerReference")]
    public string? CustomerReference { get; set; }
    
    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ErpOrderNumber")]
    public string? ErpOrderNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DeliveryInstruction")]
    public string? DeliveryInstruction { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DeliveryMethod")]
    public string? DeliveryMethod { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.CustomerEmail")]
    public string? CustomerEmail { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.QuoteNumber")]
    public string? QuoteNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderType")]
    public string? OrderType { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderTax")]
    public string? OrderTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderSubtotalExclTax")]
    public string? OrderSubtotalExclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderSubtotalInclTax")]
    public string? OrderSubtotalInclTax { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.OrderDate")]
    public string? OrderDate { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DeliveryDate")]
    public string? DeliveryDate { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.DateRequired")]
    public string? DateRequired { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpOrderSettingsModel.Fields.ShippingAmount")]
    public string? ShippingAmount { get; set; }

    public bool IsAdditionalHardCodedValuesNeeded { get; set; }
    public bool IsSkipOnUpdatePropertyNeeded { get; set; }

    public ErpOrderItemDataSettingsModel ErpOrderItemDataSettingsModel { get; set; }

    public ErpShippingAddressModel ErpShippingAddressSettingsModel { get; set; }
    public ErpBillingAddressModel ErpBillingAddressSettingsModel { get; set; }
    public ErpGetRequestSettingsModel ErpGetRequestSettingsModel { get; set; }
}

