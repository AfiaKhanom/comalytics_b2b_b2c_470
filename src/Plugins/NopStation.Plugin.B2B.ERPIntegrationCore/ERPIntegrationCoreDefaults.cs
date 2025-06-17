using Nop.Core.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore;

public static class ERPIntegrationCoreDefaults
{
    #region B2B-B2C Customer Roles

    public static string B2BCustomerRole => "B2B Customer";
    public static string B2BCustomerRoleSystemName => "B2BCustomer";
    public static string B2CCustomerRole => "B2C Customer";
    public static string B2CCustomerRoleSystemName => "B2CCustomer";
    public static string B2BQuoteAssistantRole => "B2B Quote Assistant";
    public static string B2BQuoteAssistantRoleSystemName => "B2BQuoteAssistant";
    public static string B2BOrderAssistantRole => "B2B Order Assistant";
    public static string B2BOrderAssistantRoleSystemName => "B2BOrderAssistant";
    public static string B2BB2CAdminRole => "B2B-B2C Admin";
    public static string B2BB2CAdminRoleSystemName => "B2BB2CAdmin";
    public static string B2BCustomerAccountingPersonnelRole => "B2B Customer Accounting Personnel";
    public static string B2BCustomerAccountingPersonnelRoleSystemName => "B2BCustomerAccountingPersonnel";
    public static string B2BSalesRepRole => "B2B Sales Rep";
    public static string B2BSalesRepRoleSystemName => "B2BSalesRep";
    public static string QuickOrderUserRole => "Quick Order User";
    public static string QuickOrderUserRoleSystemName => "QuickOrderUser";

    #endregion

    #region Cache keys and Prefixes

    public static CacheKey ErpProductSpecificationAttributeList => new("NopB2bB2cFeaturesAdminProductSpecificationAttributeForB2b");

    #region Erp Nop user Cache key

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Customer ID
    /// </remarks>
    public static CacheKey ErpNopUserByCustomerCacheKey => new("Nop.erpnopuser.bycustomer.{0}", ErpNopUserByCustomerCacheKeyPrefix);

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Customer ID
    /// {1} : Erp Account Id
    /// </remarks>
    public static CacheKey ErpNopUserByCustomerAndErpAccountCacheKey => new("Nop.erpnopuser.bycustomer.{0}-{1}", ErpNopUserByCustomerCacheKeyPrefix);

    /// <summary>
    /// Gets a key pattern to clear cache
    /// </summary>
    public static string ErpNopUserByCustomerCacheKeyPrefix => "Nop.erpnopuser.bycustomer.";

    #endregion

    #region Erp Nop user Account Map Cache key

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : ErpUserId
    /// </remarks>
    public static CacheKey ErpNopUserAccountMapByErpUserCacheKey => new("Nop.erpnopuseraccountmap.byerpuser.{0}");

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Erp Account
    /// </remarks>
    public static CacheKey ErpNopUserAccountMapByErpAccountCacheKey => new("Nop.erpnopuseraccountmap.byerpaccount.{0}", NopEntityCacheDefaults<ErpNopUserAccountMap>.Prefix);

    /// <summary>
    /// Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Erp Account Id
    /// {1} : Erp Nop User
    /// </remarks>
    public static CacheKey ErpNopUserAccountMapByErpAccountAndErpUserCacheKey => new("Nop.erpnopuseraccountmap.byerpaccount.{0}-{1}", NopEntityCacheDefaults<ErpNopUserAccountMap>.Prefix);

    #endregion

    #region Product Pricing Cache Key

    /// <summary>
    /// Gets a key for Erp product Special Price
    /// </summary>
    /// <remarks>
    /// {0} : Product id
    /// </remarks>
    public static CacheKey ErpProductPricingSpecialPriceByProductCacheKey => new("Nop.totals.erpproductpricing.specialprice.byproduct.{0}", ErpProductPricingPrefix);

    /// <summary>
    /// Gets a key for Erp product Special Price
    /// </summary>
    /// <remarks>
    /// {0} : Product id
    /// {1} : Erp Account id
    /// </remarks>
    public static CacheKey ErpProductPricingSpecialPriceByProductIdAndAccountCacheKey => new("Nop.totals.erpproductpricing.specialprice.byproduct.{0}-{1}", ErpProductPricingPrefix);

    /// <summary>
    /// Gets a key for Erp product Group Price
    /// </summary>
    /// <remarks>
    /// {0} : Product id
    /// </remarks>
    public static CacheKey ErpProductPricingGroupPriceByProductIdCacheKey => new("Nop.totals.erpproductpricing.groupprice.byproduct.{0}", ErpProductPricingPrefix);

    /// <summary>
    /// Gets a key for Erp product Group Price
    /// </summary>
    /// <remarks>
    /// {0} : Product id
    /// {1} : Erp Group Price Id
    /// </remarks>
    public static CacheKey ErpProductPricingGroupPriceByProductIdAndPriceGroupIdCacheKey => new("Nop.totals.erpproductpricing.groupprice.byproduct.{0}-{1}", ErpProductPricingPrefix);

    /// <summary>
    /// Gets a key pattern to clear cache
    /// </summary>
    public static string ErpProductPricingPrefix => "Nop.totals.erpproductpricing.";

    #endregion

    #region Erp Account Cache keys and Prefixes

    public static string ErpAccountPrefix => "Nop.erpaccount.";

    public static string ErpAccountByIdCacheKeyPrefix => "Nop.erpaccount.byid.";

    /// <summary>
    /// Gets a key for caching Erp Accounts by Id
    /// </summary>
    /// <remarks>
    /// {0} : Erp Account Id
    /// {1} : Filter out deleted flag
    /// </remarks>
    public static CacheKey ErpAccountByIdCacheKey => new("Nop.erpaccount.byid.{0}-{1}", ErpAccountByIdCacheKeyPrefix, ErpAccountPrefix);

    public static string ErpAccountByCustomerCacheKeyPrefix => "Nop.erpaccount.bycustomer.{0}";

    /// <summary>
    /// Gets a key for caching Erp Accounts by Customer
    /// </summary>
    /// <remarks>
    /// {0} : Customer ID
    /// {1} : Roles of the current customer
    /// </remarks>
    public static CacheKey ErpAccountByCustomerCacheKey => new("Nop.erpaccount.bycustomer.{0}-{1}", ErpAccountByCustomerCacheKeyPrefix, ErpAccountPrefix);

    public static string ErpAccountByIdWithActiveCacheKeyPrefix => "Nop.erpaccount.byidwithactive.";

    /// <summary>
    /// Gets a key for caching Active Erp Account by Id
    /// </summary>
    /// <remarks>
    /// {0} : Erp Account Id
    /// </remarks>
    public static CacheKey ErpAccountByIdWithActiveCacheKey => new("Nop.erpaccount.byidwithactive.{0}", ErpAccountByIdWithActiveCacheKeyPrefix, ErpAccountPrefix);

    #endregion

    #region Erp Warehouse Sales Org Map Cache keys and Prefixes

    public static string SalesRepOrgPrefix => "Nop.salesrep.salesreporgs.";
    public static string SalesRepOrgBySalesRepPrefix => "Nop.salesrep.salesreporgs.{0}";
    public static CacheKey SalesRepOrgCacheKey => new("Nop.salesrep.salesreporgs.{0}-{1}", SalesRepOrgBySalesRepPrefix, SalesRepOrgPrefix);  
    public static CacheKey ErpWarehouseSalesOrgMapByCodeCacheKey => new("ERPIntegration.warehouse.salesOrgMap.byWarehouseCode-{0}", ErpWarehouseSalesOrgMapByCodePrefix);
    public static string ErpWarehouseSalesOrgMapByCodePrefix => "ERPwarehouse.salesOrgMap.ByWarehouseCode";

    #endregion

    #endregion

    #region ERP Order Type

    public static string ErpB2BOrderType => "ORDER";
    public static string ErpB2BOrderFromQuoteType => "ORDER FROM QUOTE";
    public static string ErpB2COrderType => "B2CORDER";
    public static string ErpB2COrderFromQuoteType => "ORDER FROM B2C QUOTE";
    public static string ErpB2BQuoteType => "QUOTE";
    public static string ErpB2CQuoteType => "B2CQuote";

    #endregion

    #region ERP Order Status

    public static string ERPOrderStatusApproved => "Approved";
    public static string ERPOrderStatusPendingApproval => "Pending Approval";
    public static string ERPOrderStatusProcessing => "Processing";

    #endregion

    public static string NewB2BCustomerNeedsApproval => "NewB2BCustomerNeedsApproval";
    public static string ERPIntegrationPluginGroupName => "Nopstation_ErpIntegration";
}