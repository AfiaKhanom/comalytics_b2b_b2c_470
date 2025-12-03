using System;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpSpecialPrice : BaseEntity, ISoftDeletedEntity
{
    public int ErpAccountId { get; set; }
    public int NopProductId { get; set; }
    public decimal Price { get; set; }
    public decimal ListPrice { get; set; }
    public decimal PercentageOfAllocatedStock { get; set; }
    public DateTime? PercentageOfAllocatedStockResetTimeUtc { get; set; }
    public bool VolumeDiscount { get; set; }
    public decimal DiscountPerc { get; set; }
    public string PricingNote { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public DateTime? CreatedOnUtc { get; set; }
    public bool Deleted { get; set; }
}
