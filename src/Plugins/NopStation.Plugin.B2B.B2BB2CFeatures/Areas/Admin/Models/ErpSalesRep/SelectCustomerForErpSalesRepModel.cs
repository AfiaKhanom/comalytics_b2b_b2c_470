using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep;

public record SelectCustomerForErpSalesRepModel : BaseNopEntityModel
{
    public int SelectedCustomerId { get; set; }
}
