using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
public class ErpShipToAddressSettings : ISettings
{
    public string? AccountNumber { get; set; }
    public string? ShipToCode { get; set; }
    public string? ShipToName { get; set; }
    public string? Company { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? Suburb { get; set; }
    public string? County { get; set; }
    public string? Country { get; set; }
    public string? ZipPostalCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FaxNumber { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? EmailAddress { get; set; }
    public string? RepNumber { get; set; }
    public string? RepFullName { get; set; }
    public string? RepPhoneNumber { get; set; }
    public string? RepEmail { get; set; }
    public string? SalesOrgCode { get; set; }
}
