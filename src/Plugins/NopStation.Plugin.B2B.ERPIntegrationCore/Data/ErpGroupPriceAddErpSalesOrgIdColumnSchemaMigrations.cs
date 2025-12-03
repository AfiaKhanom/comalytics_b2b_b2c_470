using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/03/18 15:10:10", "NopStation.Plugin.B2B.ERPIntegrationCore.Data ErpGroupPrice Updatedon And Deleted Columns adding", MigrationProcessType.Update)]
public class ErpGroupPriceAddErpSalesOrgIdColumnSchemaMigrations : AutoReversingMigration
{
    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpGroupPrice))).Column(NameCompatibilityManager.GetColumnName(typeof(ErpGroupPrice), nameof(ErpGroupPrice.ErpSalesOrgId))).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpGroupPrice)))
                .AddColumn(NameCompatibilityManager.GetColumnName(typeof(ErpGroupPrice), nameof(ErpGroupPrice.ErpSalesOrgId)))
                .AsInt32()
                .Nullable();
        }
    }

    #endregion
}