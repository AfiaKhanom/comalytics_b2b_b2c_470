using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;

public partial class ErpWorkflowMessageService : IErpWorkflowMessageService
{
    #region Fields

    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IMessageTokenProvider _messageTokenProvider;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;
    private readonly ITokenizer _tokenizer;
    private readonly ICustomerService _customerService;
    private readonly MessagesSettings _messagesSettings;
    private readonly IAddressService _addressService;
    private readonly IAffiliateService _affiliateService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpShipToAddressService _erpShipToAddressService;

    #endregion

    #region Ctor

    public ErpWorkflowMessageService(
        EmailAccountSettings emailAccountSettings,
        IEmailAccountService emailAccountService,
        IEventPublisher eventPublisher,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        IMessageTokenProvider messageTokenProvider,
        IQueuedEmailService queuedEmailService,
        IStoreContext storeContext,
        IStoreService storeService,
        ITokenizer tokenizer,
        ICustomerService customerService,
        MessagesSettings messagesSettings,
        IAddressService addressService,
        IAffiliateService affiliateService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpShipToAddressService erpShipToAddressService)
    {
        _emailAccountSettings = emailAccountSettings;
        _emailAccountService = emailAccountService;
        _eventPublisher = eventPublisher;
        _languageService = languageService;
        _localizationService = localizationService;
        _messageTemplateService = messageTemplateService;
        _messageTokenProvider = messageTokenProvider;
        _queuedEmailService = queuedEmailService;
        _storeContext = storeContext;
        _storeService = storeService;
        _tokenizer = tokenizer;
        _customerService = customerService;
        _messagesSettings = messagesSettings;
        _addressService = addressService;
        _affiliateService = affiliateService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpShipToAddressService = erpShipToAddressService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
    {
        var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

        if (!messageTemplates?.Any() ?? true)
            return new List<MessageTemplate>();

        messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

        return messageTemplates;
    }

    protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
    {
        var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
        //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
        if (emailAccountId == 0)
            emailAccountId = messageTemplate.EmailAccountId;

        var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId)
            ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId))
            ?? (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

        return emailAccount;
    }

    protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
    {
        var language = await _languageService.GetLanguageByIdAsync(languageId);

        if (language == null || !language.Published)
        {
            language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
        }

        if (language == null || !language.Published)
        {
            language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
        }

        if (language == null)
            throw new Exception("No active language could be loaded");

        return language.Id;
    }

    /// <summary>
    /// Send notification
    /// </summary>
    /// <param name="messageTemplate">Message template</param>
    /// <param name="emailAccount">Email account</param>
    /// <param name="languageId">Language identifier</param>
    /// <param name="tokens">Tokens</param>
    /// <param name="toEmailAddress">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <param name="attachmentFilePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name</param>
    /// <param name="replyToEmailAddress">"Reply to" email</param>
    /// <param name="replyToName">"Reply to" name</param>
    /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
    /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
    /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
    /// <returns>Queued email identifier</returns>
    public virtual async Task<int> SendNotificationAsync(MessageTemplate messageTemplate,
        EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
        string toEmailAddress, string toName,
        string attachmentFilePath = null, string attachmentFileName = null,
        string replyToEmailAddress = null, string replyToName = null,
        string fromEmail = null, string fromName = null, string subject = null)
    {
        if (messageTemplate == null)
            throw new ArgumentNullException(nameof(messageTemplate));

        if (emailAccount == null)
            throw new ArgumentNullException(nameof(emailAccount));

        var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);

        if (string.IsNullOrEmpty(subject))
            subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);

        var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

        var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
        var bodyReplaced = _tokenizer.Replace(body, tokens, true);

        toName = CommonHelper.EnsureMaximumLength(toName, 300);

        var email = new QueuedEmail
        {
            Priority = QueuedEmailPriority.High,
            From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
            FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
            To = toEmailAddress,
            ToName = toName,
            ReplyTo = replyToEmailAddress,
            ReplyToName = replyToName,
            CC = string.Empty,
            Bcc = bcc,
            Subject = subjectReplaced,
            Body = bodyReplaced,
            AttachmentFilePath = attachmentFilePath,
            AttachmentFileName = attachmentFileName,
            AttachedDownloadId = messageTemplate.AttachedDownloadId,
            CreatedOnUtc = DateTime.UtcNow,
            EmailAccountId = emailAccount.Id,
            DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                : (DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
        };

        await _queuedEmailService.InsertQueuedEmailAsync(email);

        return email.Id;
    }

    protected async Task<(string email, string name)> GetStoreOwnerNameAndEmailAsync(EmailAccount messageTemplateEmailAccount)
    {
        var storeOwnerEmailAccount = _messagesSettings.UseDefaultEmailAccountForSendStoreOwnerEmails ? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId) : null;
        storeOwnerEmailAccount ??= messageTemplateEmailAccount;

        return (storeOwnerEmailAccount.Email, storeOwnerEmailAccount.DisplayName);
    }

    protected async Task<(string email, string name)> GetCustomerReplyToNameAndEmailAsync(MessageTemplate messageTemplate, Customer customer)
    {
        if (!messageTemplate.AllowDirectReply)
            return (null, null);

        var replyToEmail = await _customerService.IsGuestAsync(customer)
            ? string.Empty
            : customer.Email;

        var replyToName = await _customerService.IsGuestAsync(customer)
            ? string.Empty
            : await _customerService.GetCustomerFullNameAsync(customer);

        return (replyToEmail, replyToName);
    }

    protected async Task<(string email, string name)> GetCustomerReplyToNameAndEmailAsync(MessageTemplate messageTemplate, Order order)
    {
        if (!messageTemplate.AllowDirectReply)
            return (null, null);

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

        return (billingAddress.Email, $"{billingAddress.FirstName} {billingAddress.LastName}");
    }

    #endregion

    #region Methods

    #region Order workflow

    public async Task<int> SendERPOrderPlaceFailedSalesRepNotificationAsync(Order order, int languageId, ErpShipToAddress erpShipToAddress)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplate = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPOrderPlaceFailedSalesRepNotification, store.Id)).FirstOrDefault();

        if (messageTemplate is null)
            return 0;

        var commonTokens = new List<Token>();
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

        var tokens = new List<Token>(commonTokens);
        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

        await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

        var toEmail = erpShipToAddress.RepEmail;
        var toName = erpShipToAddress.RepFullName;

        return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
    }

    public async Task<IList<int>> SendOrderPlacedStoreOwnerNotificationAsync(Order order, int languageId)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_STORE_OWNER_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var (toEmail, toName) = await GetStoreOwnerNameAndEmailAsync(emailAccount);
            var (replyToEmail, replyToName) = await GetCustomerReplyToNameAndEmailAsync(messageTemplate, order);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                replyToEmailAddress: replyToEmail, replyToName: replyToName);
        }).ToListAsync();
    }

    public async Task<IList<int>> SendOrderPlacedVendorNotificationAsync(Order order, Vendor vendor, int languageId)
    {
        ArgumentNullException.ThrowIfNull(order);

        ArgumentNullException.ThrowIfNull(vendor);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_VENDOR_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var toEmail = vendor.Email;
            var toName = vendor.Name;

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public async Task<IList<int>> SendOrderPlacedAffiliateNotificationAsync(Order order, int languageId)
    {
        ArgumentNullException.ThrowIfNull(order);

        var affiliate = await _affiliateService.GetAffiliateByIdAsync(order.AffiliateId);

        ArgumentNullException.ThrowIfNull(affiliate);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_AFFILIATE_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var affiliateAddress = await _addressService.GetAddressByIdAsync(affiliate.AddressId);
            var toEmail = affiliateAddress.Email;
            var toName = $"{affiliateAddress.FirstName} {affiliateAddress.LastName}";

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public async Task<IList<int>> SendOrderPlacedCustomerNotificationAsync(Order order, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        #region Prepare multiple email addresses

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        var emailList = new List<(string toEmailAddress, string toName)>
        {
            (customer?.Email, $"{customer?.FirstName} {customer?.LastName}")
        };

        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser?.ErpShipToAddressId ?? 0);
        if (erpShipToAddress is not null && erpShipToAddress.EmailAddresses is not null)
        {
            var emails = erpShipToAddress.EmailAddresses.Split(';');
            foreach (var email in emails)
            {
                if (!emailList.Exists(x => x.toEmailAddress == email.Trim()))
                    emailList.Add((email.Trim(), $"{erpShipToAddress.ShipToName}"));
            }
        }

        if (!string.IsNullOrWhiteSpace(erpShipToAddress.RepEmail) &&
            !emailList.Exists(x => x.toEmailAddress == erpShipToAddress.RepEmail.Trim()))
        {
            emailList.Add((erpShipToAddress.RepEmail.Trim(), $"{erpShipToAddress.ShipToName}"));
        }

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

        if (billingAddress != null && !emailList.Exists(x => x.toEmailAddress == billingAddress.Email.Trim()))
            emailList.Add((billingAddress.Email.Trim(), $"{billingAddress.FirstName} {billingAddress.LastName}"));

        #endregion

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        var results = new List<int>();

        foreach (var email in emailList)
        {
            foreach (var messageTemplate in messageTemplates)
            {
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var result = await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens,
                    email.toEmailAddress, email.toName,
                    attachmentFilePath, attachmentFileName);

                results.Add(result);
            }
        }

        return results;
    }

    #endregion

    #region ERP Customer Registration Application

    public async Task<int> SendERPCustomerRegistrationApplicationCreatedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId)
    {
        ArgumentNullException.ThrowIfNull(applicationForm);

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
        var messageTemplateToAdmin = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToAdmin, store.Id)).FirstOrDefault();
        var messageTemplateToCustomer = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToCustomer, store.Id)).FirstOrDefault();

        var commonTokens = new List<Token>();
        await AddApplicationFormTokensAsync(commonTokens, applicationForm);

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplateToAdmin, languageId);
        var tokens = new List<Token>(commonTokens);
        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
        if (messageTemplateToAdmin is not null)
        {
            await _eventPublisher.MessageTokensAddedAsync(messageTemplateToAdmin, tokens);
            var toEmail = emailAccount.Email;
            var toName = !string.IsNullOrEmpty(emailAccount.DisplayName) ? emailAccount.DisplayName : "Admin";
            await SendNotificationAsync(messageTemplateToAdmin, emailAccount, languageId, tokens, toEmail, toName);
        }
        if (messageTemplateToCustomer is not null)
        {
            await _eventPublisher.MessageTokensAddedAsync(messageTemplateToCustomer, tokens);
            var toEmail = applicationForm.AccountsEmail;
            var toName = applicationForm.FullRegisteredName;
            await SendNotificationAsync(messageTemplateToCustomer, emailAccount, languageId, tokens, toEmail, toName);
        }
        return 0;
    }

    public async Task<int> SendERPCustomerRegistrationApplicationApprovedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId)
    {
        ArgumentNullException.ThrowIfNull(applicationForm);

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
        var messageTemplate = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationApprovedNotification, store.Id)).FirstOrDefault();

        var commonTokens = new List<Token>();
        await AddApplicationFormTokensAsync(commonTokens, applicationForm);

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
        var tokens = new List<Token>(commonTokens);
        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
        if (messageTemplate is null)
        {
            return 0;
        }

        await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
        var toEmail = applicationForm.AccountsEmail;
        var toName = applicationForm.FullRegisteredName;
        return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
    }

    public async Task AddApplicationFormTokensAsync(List<Token> tokens, ErpAccountCustomerRegistrationForm applicationForm)
    {
        tokens.Add(new Token("Application.CustomerFullName", applicationForm.FullRegisteredName));
        tokens.Add(new Token("Application.AdminName", "Admin"));
        tokens.Add(new Token("Application.Id", applicationForm.Id));
        tokens.Add(new Token("Application.RegistrationNumber", applicationForm.RegistrationNumber));
    }

    #endregion

    #endregion
}