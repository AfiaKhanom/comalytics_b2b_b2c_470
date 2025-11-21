using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpInvoiceDataSettingsModel : ODataIntegrationModel
{
    public ErpInvoiceDataSettingsModel()
    {
        ErpGetRequestSettingsModel = new ErpGetRequestSettingsModel();
    }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.PostingDateUtc")]
    public string? PostingDateUtc { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ErpDocumentNumber")]
    public string? ErpDocumentNumber { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.AmountInclVat")]
    public string? AmountInclVat { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.AmountExclVat")]
    public string? AmountExclVat { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ErpAccountId")]
    public string? ErpAccountId { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DocumentType")]
    public string? DocumentType { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DocumentDisplayName")]
    public string? DocumentDisplayName { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.PODSignedOnUtc")]
    public string? PODSignedOnUtc { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DueDateUtc")]
    public string? DueDateUtc { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.RelatedDocumentNo")]
    public string? RelatedDocumentNo { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ShipmentDateUtc")]
    public string? ShipmentDateUtc { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.DocumentDateUtc")]
    public string? DocumentDateUtc { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.ErpOrderNumber")]
    public string? ErpOrderNumber { get; set; }
    public ErpGetRequestSettingsModel ErpGetRequestSettingsModel { get; set; }
}
