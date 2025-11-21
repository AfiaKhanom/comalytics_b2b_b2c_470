using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
public class ErpInvoiceDataSettings : ISettings
{
    public string? PostingDateUtc { get; set; }
    public string? ErpDocumentNumber { get; set; }
    public string? AmountInclVat { get; set; }
    public string? AmountExclVat { get; set; }
    public string? ErpAccountId { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentDisplayName { get; set; }
    public string? PODSignedOnUtc { get; set; }
    public string? DueDateUtc { get; set; }
    public string? RelatedDocumentNo { get; set; }
    public string? ShipmentDateUtc { get; set; }
    public string? DocumentDateUtc { get; set; }
    public string? ErpOrderNumber { get; set; }
}
