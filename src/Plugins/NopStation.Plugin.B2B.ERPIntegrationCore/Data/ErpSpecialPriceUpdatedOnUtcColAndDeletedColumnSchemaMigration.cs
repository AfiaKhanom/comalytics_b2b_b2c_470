using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/03/18 15:10:10", "NopStation.Plugin.B2B.ERPIntegrationCore.Data ErpSpecialPricing Updatedon And Deleted Columns adding", MigrationProcessType.Update)]
public class ErpGroupPriceAddErpSalesOrgIdColumnSchemaMigration : AutoReversingMigration
{
    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSpecialPrice))).Column(NameCompatibilityManager.GetColumnName(typeof(ErpSpecialPrice), nameof(ErpSpecialPrice.UpdatedOnUtc))).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSpecialPrice)))
                .AddColumn(NameCompatibilityManager.GetColumnName(typeof(ErpSpecialPrice), nameof(ErpSpecialPrice.UpdatedOnUtc)))
                .AsDateTime2()
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSpecialPrice))).Column(NameCompatibilityManager.GetColumnName(typeof(ErpSpecialPrice), nameof(ErpSpecialPrice.CreatedOnUtc))).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSpecialPrice)))
                .AddColumn(NameCompatibilityManager.GetColumnName(typeof(ErpSpecialPrice), nameof(ErpSpecialPrice.CreatedOnUtc)))
                .AsDateTime2()
                .Nullable();
        }

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpSpecialPrice))).Column(NameCompatibilityManager.GetColumnName(typeof(ErpSpecialPrice), nameof(ErpSpecialPrice.Deleted))).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpSpecialPrice)))
                .AddColumn(NameCompatibilityManager.GetColumnName(typeof(ErpSpecialPrice), nameof(ErpSpecialPrice.Deleted)))
                .AsBoolean()
                .Nullable();
        }
    }

    #endregion
}