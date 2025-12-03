using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpPriceGroupProductPricingSearchModel : BaseSearchModel
    {
        public ErpPriceGroupProductPricingSearchModel()
        {
            AddErpPriceGroupProductPricing = new ErpPriceGroupProductPricingModel();
            AvailableErpSalesOrgs = new List<SelectListItem>();
        }

        public int ProductId { get; set; }
        public int ErpSalesOrgId { get; set; }
        public IList<SelectListItem> AvailableErpSalesOrgs { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.SearchErpPriceGroupCode")]
        public string SearchErpPriceGroupCode { get; set; }

        public ErpPriceGroupProductPricingModel AddErpPriceGroupProductPricing { get; set; }
    }
}
