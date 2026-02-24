using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Shipping.GeofenceDelivery.Data.Domain;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Data.Mapping;

public class GeofenceZoneMap : NopEntityBuilder<GeofenceZone>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(GeofenceZone.Name)).AsString(200).NotNullable()
            .WithColumn(nameof(GeofenceZone.CoordinatesJson)).AsString(int.MaxValue).NotNullable()
            .WithColumn(nameof(GeofenceZone.DeliveryFee)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(GeofenceZone.IsActive)).AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn(nameof(GeofenceZone.DisplayOrder)).AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn(nameof(GeofenceZone.IsCollectionOnly)).AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn(nameof(GeofenceZone.CreatedOnUtc)).AsDateTime().NotNullable()
            .WithColumn(nameof(GeofenceZone.UpdatedOnUtc)).AsDateTime().NotNullable();
    }
}
