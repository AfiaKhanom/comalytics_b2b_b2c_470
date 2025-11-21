using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record ErpGetRequestSettingsModel : AdditionalFiltersModel
{

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.AccountNumber")]
    public string? AccountNumber { get; set; }
    public bool AccountNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.DocumentNumber")]
    public string? DocumentNumber { get; set; }
    public bool DocumentNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.OrderNumber")]
    public string? OrderNumber { get; set; }
    public bool OrderNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.ProductSku")]
    public string? ProductSku { get; set; }
    public bool ProductSku_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.LastChangedDate")]
    public string? LastChangedDate { get; set; }
    public bool LastChangedDate_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.DateFrom")]
    public string? DateFrom { get; set; }
    public bool DateFrom_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.DateTo")]
    public string? DateTo { get; set; }
    public bool DateTo_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.SalesOrg")]
    public string? Location { get; set; }
    public bool Location_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.WarehouseCode")]
    public string? WarehouseCode { get; set; }
    public bool WarehouseCode_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.ErpGetRequestSettingsModel.Fields.PriceCode")]
    public string? PriceCode { get; set; }
    public bool PriceCode_OverrideForStore { get; set; }

    // limit 

    [NopResourceDisplayName("Plugins.NopStation.BCSIntegration.ErpGetRequestSettingsModel.Fields.ProductSyncLimit")]
    public int ProductSyncLimit { get; set; }
    public bool ProductSyncLimit_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.BCSIntegration.ErpGetRequestSettingsModel.Fields.AccountSyncLimit")]
    public int AccountSyncLimit { get; set; }
    public bool AccountSyncLimit_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.BCSIntegration.ErpGetRequestSettingsModel.Fields.StockSyncLimit")]
    public int StockSyncLimit { get; set; }
    public bool StockSyncLimit_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.BCSIntegration.ErpGetRequestSettingsModel.Fields.OrderSyncLimit")]
    public int OrderSyncLimit { get; set; }
    public bool OrderSyncLimit_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.BCSIntegration.ErpGetRequestSettingsModel.Fields.InvoiceSyncLimit")]
    public int InvoiceSyncLimit { get; set; }
    public bool InvoiceSyncLimit_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.BCSIntegration.ErpGetRequestSettingsModel.Fields.SpecialPriceSyncLimit")]
    public int SpecialPriceSyncLimit { get; set; }
    public bool SpecialPriceSyncLimit_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.BCSIntegration.ErpGetRequestSettingsModel.Fields.GroupPriceSyncLimit")]
    public int GroupPriceSyncLimit { get; set; }
    public bool GroupPriceSyncLimit_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.BCSIntegration.ErpGetRequestSettingsModel.Fields.ShipToAddressSyncLimit")]
    public int ShipToAddressSyncLimit { get; set; }
    public bool ShipToAddressSyncLimit_OverrideForStore { get; set; }
}
