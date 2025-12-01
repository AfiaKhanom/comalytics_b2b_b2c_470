CREATE TABLE dbo.ErpNopUserImport
(
    Id                              NVARCHAR(100)    NULL,
    Email                           NVARCHAR(1000)   NULL,
    AccountNumber                   NVARCHAR(50)     NULL,
    AccountName                     NVARCHAR(100)    NULL,
    AccountSalesOrganisationCode    NVARCHAR(100)    NULL,
    ShipToCode                      NVARCHAR(50)     NULL,
    ShipToName                      NVARCHAR(100)    NULL,
    IsActive                        NVARCHAR(10)     NULL,
    ErpUserType                     NVARCHAR(100)    NULL
);