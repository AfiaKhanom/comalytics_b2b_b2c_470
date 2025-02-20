using FluentMigrator;
using Nop.Core;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data;

[NopMigration("2025/01/19 20:12:00", "NopStation.Plugin.B2B.ERPIntegrationCore ErpSalesOrg Added column Password", MigrationProcessType.Update)]
public class ErpSalesOrgPasswordAddColumnMigration : AutoReversingMigration
{
    public static string TableName<T>() where T : BaseEntity
    {
        return NameCompatibilityManager.GetTableName(typeof(T));
    }

    public override void Up()
    {
        var erpSalesOrgTableName = TableName<ErpSalesOrg>();

        if (Schema.Table(erpSalesOrgTableName).Exists() && !Schema.Table(erpSalesOrgTableName).Column(nameof(ErpSalesOrg.Password)).Exists())
        {
            Alter.Table(erpSalesOrgTableName)
                .AddColumn(NameCompatibilityManager.GetColumnName(typeof(ErpSalesOrg), nameof(ErpSalesOrg.Password)))
                .AsString()
                .Nullable();
        }        
    }
}
