using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Mapping.Builders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using System.Data;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Builders;

public class ErpShipToAddressBuilder : NopEntityBuilder<ErpShipToAddress>
{
    #region Methods

    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.ShipToCode))).AsString()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.ShipToName))).AsString()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.AddressId))).AsInt32().ForeignKey<Address>(onDelete: Rule.None)
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.ProvinceCode))).AsString().Nullable()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.DeliveryNotes))).AsString().Nullable()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.EmailAddresses))).AsString().Nullable()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.RepNumber))).AsString()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.RepFullName))).AsString().Nullable()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.RepPhoneNumber))).AsString().Nullable()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.RepEmail))).AsString().Nullable()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.Suburb))).AsString().Nullable()
            .WithColumn(NameCompatibilityManager.GetColumnName(typeof(ErpShipToAddress), nameof(ErpShipToAddress.LastShipToAddressSyncDate))).AsDateTime2().Nullable();
    }

    #endregion
}
