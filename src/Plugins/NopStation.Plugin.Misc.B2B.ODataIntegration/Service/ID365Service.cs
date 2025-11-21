using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Services
{
    public interface ID365Service
    {
        bool IsValidD365IntegrationApiUrlSettings(D365IntegrationApiUrlSettings settings, string requestUrl);
        void AddOrUpdateAdditionalMapping(string key, string value);
        void AddOrUpdateDefaultDateTimeFormat(string value);

        Task LoadProductAttributeMappings();
        Task<ErpResponseData<IList<ErpAccountDataModel>>> GetCustomersFromErpAsync(ErpGetRequestModel erpRequest);
        Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel);
        Task<ErpResponseData<ErpShipToAddressDataModel>> CreateShipToAddressOnErpAsync(ErpShipToAddressCreateModel erpShipToAddressCreateModel);


        Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest);
        Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest);

        Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrdersByAccountFromErpAsync(ErpGetRequestModel erpRequest);

        Task<ErpResponseData<Dictionary<int, string>>> CreateOrderAsync(ErpPlaceOrderDataModel orderData, List<int> orderItemIds = null);

        #region Invoice

        Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest);
        Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest);
        //Task<ErpResponseData<string>> GetStatementPdfByteCodeFromErpAsync(ErpGetRequestModel erpRequest);

        #endregion

        #region Ship To Address

        Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressFromErpAsync(ErpGetRequestModel erpRequest);

        #endregion

        #region Pricing
        Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetPerAccountProductPricingFromErpAsync(ErpGetRequestModel erpRequest);
        Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricingFromErpAsync(ErpGetRequestModel erpRequest);
        #endregion

        (bool IsValid, string ErrorMessage) ValidateAdditionalHardCodedValuesSettings(string jsonSetting, bool checkType = true);
        (bool IsValid, string ErrorMessage) ValidateTranslationTableModelSettings(string jsonSetting);
        (bool IsValid, string ErrorMessage) ValidateProductAttributeMappings(string jsonSetting);


    }
}