using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
public record D365IntegrationApiUrlSettingModel : BaseNopModel, ISettingsModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.AccessTokenUrl")]
    public string? AccessTokenUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ClientId")]
    public string? ClientId { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ClientSecret")]
    public string? ClientSecret { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.Scope")]
    public string? Scope { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ProductApiUrl")]
    public string? ProductApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.AccountApiUrl")]
    public string? AccountApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.ShippingAddressApiUrl")]
    public string? ShippingAddressApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.InvoiceApiUrl")]
    public string? InvoiceApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.InvoicePdfApiUrl")]
    public string? InvoicePdfApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.OrderApiUrl")]
    public string? OrderApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.SpecialPriceApiUrl")]
    public string? SpecialPriceApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.GroupPriceApiUrl")]
    public string? GroupPriceApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.StockApiUrl")]
    public string? StockApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.PlaceOrderHeaderApiUrl")]
    public string? PlaceOrderHeaderApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.IsOrderItemExcluded")]
    public bool IsOrderItemExcluded { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.PlaceOrderLinesApiUrl")]
    public string? PlaceOrderLinesApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.CreateAccountApiUrl")]
    public string? CreateAccountApiUrl { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.D365Integration.D365IntegrationApiUrlSettingModel.Fields.CreateShipToAddressApiUrl")]
    public string? CreateShipToAddressApiUrl { get; set; }
}

