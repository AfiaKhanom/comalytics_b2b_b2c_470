using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings.PlaceOrderSettings;
public class ErpPlaceOrderSettings : AdditionalHardcodedValueSettingsModel
{
    public string? AccountNumber { get; set; }
    public string? Location { get; set; }
    public string? CustomOrderNumber { get; set; }
    public string? RepCode { get; set; }
    public string? AddressCode { get; set; }
    public string? CustomerReference { get; set; }
    public string? ErpOrderNumber { get; set; }
    public string? DeliveryInstruction { get; set; }
    public string? DeliveryMethod { get; set; }
    public string? CustomerEmail { get; set; }
    public string? OrderType { get; set; }
    public string? QuoteNumber { get; set; }
    public string? OrderTax { get; set; }
    public string? OrderSubtotalExclTax { get; set; }
    public string? OrderSubtotalInclTax { get; set; }
    public string? OrderDate { get; set; }
    public string? DeliveryDate { get; set; }
    public string? DateRequired { get; set; }
}
