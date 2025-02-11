using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;
/// <summary>
/// Customer service
/// </summary>
public partial class OverridenCustomerService : CustomerService
{
    #region Fields

    private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IRepository<ErpNopUserAccountMap> _erpNopUserAccountMap;

    #endregion

    #region Ctor

    public OverridenCustomerService(CustomerSettings customerSettings,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        INopDataProvider dataProvider,
        IRepository<Address> customerAddressRepository,
        IRepository<BlogComment> blogCommentRepository,
        IRepository<Customer> customerRepository,
        IRepository<CustomerAddressMapping> customerAddressMappingRepository,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IRepository<CustomerPassword> customerPasswordRepository,
        IRepository<CustomerRole> customerRoleRepository,
        IRepository<ForumPost> forumPostRepository,
        IRepository<ForumTopic> forumTopicRepository,
        IRepository<GenericAttribute> gaRepository,
        IRepository<NewsComment> newsCommentRepository,
        IRepository<Order> orderRepository,
        IRepository<ProductReview> productReviewRepository,
        IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
        IRepository<PollVotingRecord> pollVotingRecordRepository,
        IRepository<ShoppingCartItem> shoppingCartRepository,
        IShortTermCacheManager shortTermCacheManager,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        ShoppingCartSettings shoppingCartSettings,
        TaxSettings taxSettings,
        IErpNopUserAccountMapService erpNopUserAccountMapService,
        IErpNopUserService erpNopUserService,
        IRepository<ErpNopUserAccountMap> erpNopUserAccountMap) : base(customerSettings,
            eventPublisher,
            genericAttributeService,
            dataProvider,
            customerAddressRepository,
            blogCommentRepository,
            customerRepository,
            customerAddressMappingRepository,
            customerCustomerRoleMappingRepository,
            customerPasswordRepository,
            customerRoleRepository,
            forumPostRepository,
            forumTopicRepository,
            gaRepository,
            newsCommentRepository,
            orderRepository,
            productReviewRepository,
            productReviewHelpfulnessRepository,
            pollVotingRecordRepository,
            shoppingCartRepository,
            shortTermCacheManager,
            staticCacheManager,
            storeContext,
            shoppingCartSettings,
            taxSettings)
    {
        _erpNopUserAccountMapService = erpNopUserAccountMapService;
        _erpNopUserService = erpNopUserService;
        _erpNopUserAccountMap = erpNopUserAccountMap;
    }

    #endregion

    #region Methods

    #region Customer roles

    /// <summary>
    /// Gets list of customer roles
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="showHidden">A value indicating whether to load hidden records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public override async Task<IList<CustomerRole>> GetCustomerRolesAsync(Customer customer, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var allRolesById = await GetAllCustomerRolesDictionaryAsync();

        var mappings = await _shortTermCacheManager.GetAsync(
            async () => await _customerCustomerRoleMappingRepository.GetAllAsync(query => query.Where(crm => crm.CustomerId == customer.Id)), NopCustomerServicesDefaults.CustomerRolesCacheKey, customer);

        var nopCustomerRoles = mappings
            .Select(mapping => allRolesById.TryGetValue(mapping.CustomerRoleId, out var role) ? role : null)
            .Where(cr => cr != null && (showHidden || cr.Active))
            .ToList();

        return nopCustomerRoles;
    }

    /// <summary>
    /// Get customer role identifiers
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="showHidden">A value indicating whether to load hidden records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer role identifiers
    /// </returns>
    public override async Task<int[]> GetCustomerRoleIdsAsync(Customer customer, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var customerRoleIds = (await GetCustomerRolesAsync(customer, showHidden: showHidden)).Select(cr => cr.Id).ToArray();

        #region B2B

        var currentErpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);

        if (currentErpUser == null)
            return customerRoleIds;

        var erpAccountNopUserMap = await _erpNopUserAccountMapService.GetErpNopUserAccountMapByAccountAndUserIdAsync(currentErpUser.ErpAccountId, currentErpUser.Id);
        var erpUserCustomerRoleIds = new List<int>();

        if (erpAccountNopUserMap != null && !string.IsNullOrWhiteSpace(erpAccountNopUserMap.CustomerRolesIds))
        {
            erpUserCustomerRoleIds = erpAccountNopUserMap.CustomerRolesIds
                .Split(',')
                .Select(y => int.TryParse(y, out var roleId) ? roleId : (int?)null)
                .Where(roleId => roleId.HasValue)
                .Select(roleId => roleId.Value)
                .ToList();
        }

        var allCustomerRoleIds = customerRoleIds.Concat(erpUserCustomerRoleIds).Distinct().ToArray();

        #endregion

        return allCustomerRoleIds;
    }

    #endregion

    #endregion
}