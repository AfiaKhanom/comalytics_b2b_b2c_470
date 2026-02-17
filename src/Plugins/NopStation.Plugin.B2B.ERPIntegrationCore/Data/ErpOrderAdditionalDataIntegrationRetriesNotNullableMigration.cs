using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2026/02/17 10:00:00", "NopStation.Plugin.B2B.ERPIntegrationCore ErpOrderAdditionalData IntegrationRetries NotNullable", MigrationProcessType.NoMatter)]
public class ErpOrderAdditionalDataIntegrationRetriesNotNullableMigration : Migration
{
    #region Methods

    public override void Up()
    {
        var tableName = NameCompatibilityManager.GetTableName(typeof(ErpOrderAdditionalData));
        var columnName = NameCompatibilityManager.GetColumnName(
            typeof(ErpOrderAdditionalData),
            nameof(ErpOrderAdditionalData.IntegrationRetries));

        if (Schema.Table(tableName).Column(columnName).Exists())
        {
            Execute.Sql(
                $"UPDATE [{tableName}] " +
                $"SET [{columnName}] = 0 " +
                $"WHERE [{columnName}] IS NULL");

            Alter.Table(tableName)
                .AlterColumn(columnName)
                .AsInt32()
                .NotNullable()
                .WithDefaultValue(0);
        }
    }
    public override void Down() { }

    #endregion
}
