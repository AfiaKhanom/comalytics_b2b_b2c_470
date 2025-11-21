using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpInvoicePdfSettingsModel : BaseNopModel, ISettingsModel
{
    public ErpInvoicePdfSettingsModel()
    {
        ErpGetRequestSettingsModel = new ErpGetRequestSettingsModel();
    }
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpInvoiceDataSettingsModel.Fields.InvoicePDFData")]
    public string? InvoicePDFData { get; set; }
    public ErpGetRequestSettingsModel ErpGetRequestSettingsModel { get; set; }
}
