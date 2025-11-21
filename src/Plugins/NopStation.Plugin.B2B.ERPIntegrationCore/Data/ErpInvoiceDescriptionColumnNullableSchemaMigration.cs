using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/10/10 12:12:12", "NopStation.Plugin.B2B.ERPIntegrationCore ErpInvoice Description Column Nullable", MigrationProcessType.Update)]

public class ErpInvoiceDescriptionColumnNullableSchemaMigration : AutoReversingMigration
{
    #region Methods

    public override void Up()
    {
        if (Schema.Table(NameCompatibilityManager.GetTableName(typeof(ErpInvoice)))
            .Column(NameCompatibilityManager.GetColumnName(typeof(ErpInvoice), nameof(ErpInvoice.Description))).Exists())
        {
            Alter.Table(NameCompatibilityManager.GetTableName(typeof(ErpInvoice)))
            .AlterColumn(NameCompatibilityManager.GetColumnName(typeof(ErpInvoice), nameof(ErpInvoice.Description)))
            .AsString(int.MaxValue)
            .Nullable();
        }
    }

    #endregion
}