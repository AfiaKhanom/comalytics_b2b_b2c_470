CREATE TABLE ErpShipToAddressImport
(
    Id                          NVARCHAR(100)    NULL,
    ShipToCode                  NVARCHAR(50)     NULL,
    ShipToName                  NVARCHAR(100)    NULL,

    Company                     NVARCHAR(MAX)    NULL,
    Country                     NVARCHAR(MAX)    NULL,
    StateProvince               NVARCHAR(MAX)    NULL,
    City                        NVARCHAR(MAX)    NULL,
    Address1                    NVARCHAR(MAX)    NULL,
    Address2                    NVARCHAR(MAX)    NULL,
    Suburb                      NVARCHAR(200)    NULL,
    ZipPostalCode               NVARCHAR(MAX)    NULL,
    PhoneNumber                 NVARCHAR(MAX)    NULL,
    DeliveryNotes               NVARCHAR(MAX)    NULL,
    EmailAddresses              NVARCHAR(MAX)    NULL,

    AccountNumber               NVARCHAR(50)     NULL,
    AccountSalesOrganisationCode NVARCHAR(100)   NULL,
    SalesOrganisationCode       NVARCHAR(10)     NULL,

    IsActive                    NVARCHAR(10)     NULL
);
