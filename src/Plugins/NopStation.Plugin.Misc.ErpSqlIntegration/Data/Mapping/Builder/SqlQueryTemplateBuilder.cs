using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Mapping.Builder;

public partial class SqlQueryTemplateBuilder : NopEntityBuilder<SqlQueryTemplate>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SqlQueryTemplate.Query)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(SqlQueryTemplate.ErpSyncLevelId)).AsInt32().NotNullable();
    }

    #endregion
}
