using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpStockSettingModel : ODataIntegrationModel
{

    public ErpStockSettingModel()
    {
        ErpGetRequestSettingsModel = new ErpGetRequestSettingsModel();
    }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.WarehouseNameOrCode")]
    public string? WarehouseNameOrCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.Sku")]
    public string? Sku { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.SalesOrgCode")]
    public string? SalesOrgCode { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.QuantityOnHand")]
    public string? QuantityOnHand { get; set; }
    
    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.QuantityOnSalesOrder")]
    public string? QuantityOnSalesOrder { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpStockSettingModel.Fields.LastChangedDate")]
    public string? LastChangedDate { get; set; }

    public ErpGetRequestSettingsModel ErpGetRequestSettingsModel { get; set; }
}

