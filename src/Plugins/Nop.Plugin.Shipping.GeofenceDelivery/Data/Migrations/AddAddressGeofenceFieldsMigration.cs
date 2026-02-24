using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Shipping.GeofenceDelivery.Data.Migrations;

[NopMigration("2024/01/01 00:01:00:1687541", "Shipping.GeofenceDelivery address fields", MigrationProcessType.Installation)]
public class AddAddressGeofenceFieldsMigration : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table("Address").Column("Latitude").Exists())
            Alter.Table("Address").AddColumn("Latitude").AsDecimal(10, 7).Nullable();
        if (!Schema.Table("Address").Column("Longitude").Exists())
            Alter.Table("Address").AddColumn("Longitude").AsDecimal(10, 7).Nullable();
        if (!Schema.Table("Address").Column("IsGeofenceValidated").Exists())
            Alter.Table("Address").AddColumn("IsGeofenceValidated").AsBoolean().NotNullable().WithDefaultValue(false);
        if (!Schema.Table("Address").Column("GeofenceValidationDate").Exists())
            Alter.Table("Address").AddColumn("GeofenceValidationDate").AsDateTime().Nullable();
        if (!Schema.Table("Address").Column("AssignedZoneId").Exists())
            Alter.Table("Address").AddColumn("AssignedZoneId").AsInt32().Nullable();
        if (!Schema.Table("Address").Column("IsLocationFlagged").Exists())
            Alter.Table("Address").AddColumn("IsLocationFlagged").AsBoolean().NotNullable().WithDefaultValue(false);
    }
}
