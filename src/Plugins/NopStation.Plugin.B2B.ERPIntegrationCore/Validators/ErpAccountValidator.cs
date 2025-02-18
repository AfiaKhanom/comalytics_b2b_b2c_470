using FluentValidation;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Validators;

public class ErpAccountValidator : BaseNopValidator<ErpAccount>
{
    #region Ctor

    public ErpAccountValidator()
    {
        // Required string fields with length validation
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(100);

        // Optional string fields with length validation
        RuleFor(x => x.BillingSuburb)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.BillingSuburb));

        RuleFor(x => x.VatNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.VatNumber));

        RuleFor(x => x.PreFilterFacets)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.PreFilterFacets));

        RuleFor(x => x.PaymentTypeCode)
            .MaximumLength(10)
            .When(x => !string.IsNullOrEmpty(x.PaymentTypeCode));

        RuleFor(x => x.EmailAddresses)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.EmailAddresses));

        // Required integer fields
        RuleFor(x => x.ErpSalesOrgId)
            .GreaterThan(0);

        RuleFor(x => x.ErpAccountStatusTypeId)
            .GreaterThan(0);

        RuleFor(x => x.StockDisplayFormatTypeId)
            .GreaterThan(0);

        // Optional integer fields
        RuleFor(x => x.B2BPriceGroupCodeId)
            .GreaterThanOrEqualTo(0)
            .When(x => x.B2BPriceGroupCodeId.HasValue);

        RuleFor(x => x.BillingAddressId)
            .GreaterThan(0)
            .When(x => x.BillingAddressId.HasValue);

        // Required decimal fields
        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CreditLimitAvailable)
            .GreaterThanOrEqualTo(0);

        // Optional decimal fields
        RuleFor(x => x.LastPaymentAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LastPaymentAmount.HasValue);

        RuleFor(x => x.PercentageOfStockAllowed)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PercentageOfStockAllowed.HasValue);

        RuleFor(x => x.TotalSavingsForthisYear)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TotalSavingsForthisYear.HasValue);

        RuleFor(x => x.TotalSavingsForAllTime)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TotalSavingsForAllTime.HasValue);

        // Required boolean fields
        RuleFor(x => x.AllowOverspend).NotNull();
        RuleFor(x => x.OverrideBackOrderingConfigSetting).NotNull();
        RuleFor(x => x.AllowAccountsBackOrdering).NotNull();
        RuleFor(x => x.OverrideAddressEditOnCheckoutConfigSetting).NotNull();
        RuleFor(x => x.AllowAccountsAddressEditOnCheckout).NotNull();
        RuleFor(x => x.OverrideStockDisplayFormatConfigSetting).NotNull();
        RuleFor(x => x.IsDefaultPaymentAccount).NotNull();
    }

    #endregion
}