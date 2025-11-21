using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.B2B.ODataIntegration.GetRequestSettings;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Settings;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Settings.PlaceOrderSettings;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<D365IntegrationSettings, ConfigurationModel>()
                .ForMember(model => model.DefaultCustomerId_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ErpCallTimeOut_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.HttpCallMaxRetries_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.HttpCallRestTimeInMinutes_OverrideForStore, options => options.Ignore());

            CreateMap<ConfigurationModel, D365IntegrationSettings>();

            CreateMap<D365IntegrationApiUrlSettingModel, D365IntegrationApiUrlSettings>().ReverseMap();
            CreateMap<ErpAccountSettingModel, ErpAccountSetting>().ReverseMap();
            CreateMap<ErpProductSettingModel, ErpProductSettings>().ReverseMap();
            CreateMap<ErpCategoryDataSettingsModel, ErpCategoryDataSettings>().ReverseMap();
            CreateMap<ErpShipToAddressSettingModel, ErpShipToAddressSettings>().ReverseMap();
            CreateMap<ErpStockSettingModel, ErpStockSettings>().ReverseMap();

            CreateMap<ErpOrderSettingsModel, ErpOrderSettings>().ReverseMap();
            CreateMap<ErpOrderItemDataSettingsModel, ErpOrderItemDataSettings>().ReverseMap();

            CreateMap<ErpShippingAddressModel, ErpOrderShippingAddressSettings>().ReverseMap();
            CreateMap<ErpBillingAddressModel, ErpOrderBillingAddressSettings>().ReverseMap();

            CreateMap<ErpInvoiceDataSettingsModel, ErpInvoiceDataSettings>().ReverseMap();
            CreateMap<ErpPriceSpecialPricingDataSettingsModel, ErpPriceSpecialPricingDataSettings>().ReverseMap();
            CreateMap<ErpPriceGroupPricingDataSettingsModel, ErpPriceGroupPricingDataSettings>().ReverseMap();
            CreateMap<ErpInvoicePdfSettingsModel, ErpInvoicePdfSettings>().ReverseMap();


            //Request mapping 
            CreateMap<ErpGetRequestSettingsModel, ErpAccountGetRequestSettings>().ReverseMap();
            CreateMap<ErpGetRequestSettingsModel, ErpProductGetRequestSettings>().ReverseMap();
            CreateMap<ErpGetRequestSettingsModel, ErpStockGetRequestSettings>().ReverseMap();
            CreateMap<ErpGetRequestSettingsModel, ErpShipToAddressGetRequestSettings>().ReverseMap();
            CreateMap<ErpGetRequestSettingsModel, ErpOrderGetRequestSettings>().ReverseMap();
            CreateMap<ErpGetRequestSettingsModel, ErpSpecialPriceGetRequestSettings>().ReverseMap();
            CreateMap<ErpGetRequestSettingsModel, ErpGroupPriceGetRequestSettings>().ReverseMap();
            CreateMap<ErpGetRequestSettingsModel, ErpInvoiceGetRequestSettings>().ReverseMap();
            CreateMap<ErpGetRequestSettingsModel, ErpInvoicePdfGetRequestSettings>().ReverseMap();

            //place Order
            CreateMap<ErpOrderSettingsModel, ErpPlaceOrderSettings>().ReverseMap();
            CreateMap<ErpOrderItemDataSettingsModel, ErpPlaceOrderItemSettings>().ReverseMap();
            CreateMap<ErpShippingAddressModel, ErpPlaceOrderShippingAddressSettings>().ReverseMap();
            CreateMap<ErpBillingAddressModel, ErpPlaceOrderBillingAddressSettings>().ReverseMap();

            //Create Account Mapping

            CreateMap<ErpCreateAccountSettingsModel, ErpCreateAccountSettings>().ReverseMap();

            CreateMap<ErpCreateShipToAddressSettingsModel, ErpCreateShipToAddressSettings>().ReverseMap();

        }

        public int Order => 1;
    }
}