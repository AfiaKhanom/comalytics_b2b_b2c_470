CREATE TABLE ErpAccountImport
(
    Id                          NVARCHAR(100)   NULL,
    AccountNumber               NVARCHAR(50)    NULL,
    AccountName                 NVARCHAR(100)   NULL,
    SalesOrganisationCode       NVARCHAR(100)   NULL,

    BillingFirstName            NVARCHAR(MAX)   NULL,
    BillingLastName             NVARCHAR(MAX)   NULL,
    BillingEmail                NVARCHAR(MAX)   NULL,
    BillingCompany              NVARCHAR(MAX)   NULL,
    BillingCountry              NVARCHAR(MAX)   NULL,
    BillingStateProvince        NVARCHAR(MAX)   NULL,
    BillingCity                 NVARCHAR(MAX)   NULL,
    BillingAddress1             NVARCHAR(MAX)   NULL,
    BillingAddress2             NVARCHAR(MAX)   NULL,
    BillingSuburb               NVARCHAR(200)   NULL,
    BillingZipPostalCode        NVARCHAR(MAX)   NULL,
    BillingPhoneNumber          NVARCHAR(MAX)   NULL,

    VatNumber                   NVARCHAR(50)    NULL,
    CreditLimit                 NVARCHAR(100)   NULL,
    CreditLimitAvailable        NVARCHAR(100)   NULL,
    CurrentBalance              NVARCHAR(100)   NULL,

    AllowOverspend              NVARCHAR(10)    NULL,
    PriceGroupCode              NVARCHAR(50)    NULL,
    PreFilterFacets             NVARCHAR(500)   NULL,
    PaymentTypeCode             NVARCHAR(10)    NULL,

    OverrideBackOrderingConfigSetting        NVARCHAR(10) NULL,
    AllowAccountsBackOrdering                NVARCHAR(10) NULL,
    OverrideStockDisplayFormatConfigSetting  NVARCHAR(10) NULL,

    OverrideAddressEditOnCheckoutConfigSetting  NVARCHAR(10) NULL,
    AllowAccountsAddressEditOnCheckout          NVARCHAR(10) NULL,

    StockDisplayFormatTypeId   NVARCHAR(10)    NULL,
    ErpAccountStatusTypeId     NVARCHAR(10)    NULL,
    PercentageOfStockAllowed   NVARCHAR(10)    NULL,

    LastAccountRefresh         NVARCHAR(MAX)   NULL,
    LastPriceRefresh           NVARCHAR(MAX)   NULL,

    IsActive                   NVARCHAR(10)    NULL,
    IsDefaultPaymentAccount    NVARCHAR(10)    NULL
);
