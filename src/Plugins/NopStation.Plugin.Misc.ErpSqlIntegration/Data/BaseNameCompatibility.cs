using Nop.Data.Mapping;
using NopStation.Plugin.Misc.ErpSqlIntegration.Domain;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(SqlQueryTemplate), "SQLQueryTemplates" }
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {

        };
    }
}