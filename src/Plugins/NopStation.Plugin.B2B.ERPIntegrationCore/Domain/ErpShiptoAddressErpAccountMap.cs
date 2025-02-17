using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpShiptoAddressErpAccountMap : BaseEntity
{
    public int ErpAccountId { get; set; }
    public int ErpShiptoAddressId { get; set; }
}
