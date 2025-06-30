using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Domain;

public partial class SqlQueryTemplate : BaseEntity
{
    public int ErpSyncLevelId { get; set; }

    public string Query { get; set; }

    public ErpSyncLevel ErpSyncLevel
    {
        get => (ErpSyncLevel)ErpSyncLevelId;
        set => ErpSyncLevelId = (int)value;
    }
}