namespace NopStation.Plugin.Misc.B2B.ODataIntegration;

public class ErpCreateShipToAddressSettings : AdditionalHardcodedValueSettingsModel
{
    public string? AccountNumber { get; set; }
    public string? ShipToCode { get; set; }
    public string? ShipToName { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? PostalCode { get; set; }
    public string? ShipToPhone { get; set; }
    public string? ShipToEmail { get; set; }
}
