namespace NopStation.Plugin.Misc.ErpSqlIntegration.Settings;

public class ErpPlaceOrderSettings : AdditionalHardcodedValueSettings
{
    public string? AccountNumber { get; set; }
    public string? Location { get; set; }
    public string? CustomOrderNumber { get; set; }
    public string? RepCode { get; set; }
    public string? AddressCode { get; set; }
    public string? CustomerName { get; set; }
    public string? Notes { get; set; }
    public string? CustomerReference { get; set; }
    public string? DeliveryInstruction { get; set; }
    public string? OrderCategory { get; set; }
    public string? DeliveryMethod { get; set; }
    public string? CustomerFirstName { get; set; }
    public string? CustomerLastName { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public string? CustomerMobileNumber { get; set; }
    public string? CustomerEmail { get; set; }
    public string? VatNumber { get; set; }
    public string? OrderType { get; set; }
    public string? QuoteNumber { get; set; }
    public string? OrderTax { get; set; }
    public string? OrderSubtotalExclTax { get; set; }
    public string? OrderSubtotalInclTax { get; set; }
    public string? CustomerCurrencyCode { get; set; }
    public string? OrderDate { get; set; }
    public string? DeliveryDate { get; set; }
    public string? DateRequired { get; set; }
    public string? AccountName { get; set; }
    public string? ShippingAmount { get; set; }
    public string? ErpOrderPayloadRootKey { get; set; }
}