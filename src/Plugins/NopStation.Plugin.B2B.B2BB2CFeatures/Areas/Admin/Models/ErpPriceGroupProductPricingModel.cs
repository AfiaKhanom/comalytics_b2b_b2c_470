using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpPriceGroupProductPricingModel : BaseNopEntityModel
{
    public ErpPriceGroupProductPricingModel()
    {
        AvailableErpPriceGroupCodes = new List<SelectListItem>();
        AvailableErpSalesOrgs = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.Product")]
    public int ProductId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.Product")]
    public string ProductSku { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.ErpSalesOrg")]
    public int ErpSalesOrgId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.ErpSalesOrgCode")]
    public string ErpSalesOrgCode { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.PriceGroupCode")]
    public int ErpGroupPriceCodeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.PriceGroupCode")]
    public string ErpGroupPriceCode { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.Price")]
    public decimal Price { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.IsActive")]
    public bool IsActive { get; set; }

    public IList<SelectListItem> AvailableErpPriceGroupCodes { get; set; }
    public IList<SelectListItem> AvailableErpSalesOrgs { get; set; }
}
