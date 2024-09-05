using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
public class ErpAccountPictureMapping : BaseEntity
{
    public int PictureId { get; set; }
    public int ErpAccountId { get; set; }
}
