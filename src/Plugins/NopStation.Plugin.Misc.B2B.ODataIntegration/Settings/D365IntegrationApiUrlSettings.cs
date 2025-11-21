using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Settings
{
    public class D365IntegrationApiUrlSettings : ISettings
    {
        public string AccessTokenUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string ProductApiUrl { get; set; }
        public string AccountApiUrl { get; set; }
        public string ShippingAddressApiUrl { get; set; }
        public string InvoiceApiUrl { get; set; }
        public string InvoicePdfApiUrl { get; set; }
        public string OrderApiUrl { get; set; }
        public string SpecialPriceApiUrl { get; set; }
        public string GroupPriceApiUrl { get; set; }
        public string StockApiUrl { get; set; }
        public string PlaceOrderHeaderApiUrl { get; set; }
        public string PlaceOrderLinesApiUrl { get; set; }
        public string CreateAccountApiUrl { get; set; }
        public string CreateShipToAddressApiUrl { get; set; }

        public bool IsOrderItemExcluded { get; set; }
    }
}