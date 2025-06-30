using FluentMigrator;
using Nop.Core;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Data.Migration;

[NopMigration("2023/10/18 12:00:00", "Erp sql query integration", MigrationProcessType.Update)]
public class SchemaMigration : AutoReversingMigration
{
    public static string TableName<T>() where T : BaseEntity
    {
        return NameCompatibilityManager.GetTableName(typeof(T));
    }

    public override void Up()
    {
        if (!Schema.Table(TableName<SqlQueryTemplate>()).Exists())
            Create.TableFor<SqlQueryTemplate>();
    }
}