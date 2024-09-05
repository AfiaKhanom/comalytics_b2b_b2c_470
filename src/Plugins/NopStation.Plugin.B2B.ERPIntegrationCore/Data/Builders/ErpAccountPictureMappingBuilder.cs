using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Mapping;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using Nop.Data.Extensions;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;
public class ErpAccountPictureMappingBuilder : NopEntityBuilder<ErpAccount>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpAccountPictureMapping), nameof(ErpAccountPictureMapping.ErpAccountId))).AsInt32().ForeignKey<ErpAccount>(onDelete: Rule.None)
            .WithColumn(nameof(ErpAccountPictureMapping.PictureId)).AsInt32().NotNullable();
    }
    #endregion
}
