using System.Dynamic;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Services;

/// <summary>
/// Represents SysproIntegration manager
/// </summary>
public partial class SysproIntegrationService : ISysproIntegrationService
{
    private readonly SysproIntegrationSettings _sysproIntegrationSettings;

    public SysproIntegrationService(SysproIntegrationSettings sysproIntegrationSettings)
    {
        _sysproIntegrationSettings = sysproIntegrationSettings;
    }

    public Task<bool> IsValidSysproIntegrationSettings()
    {
        if (string.IsNullOrEmpty(_sysproIntegrationSettings.Token) ||
            string.IsNullOrEmpty(_sysproIntegrationSettings.BaseUrl))
        {
            return Task.FromResult(false);
        }

        if (!Uri.IsWellFormedUriString(_sysproIntegrationSettings.BaseUrl, UriKind.Absolute))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    //public async Task<SysproRequestModel> PrepareErpAccountsRequestBody(ErpGetRequestModel erpRequest)
    //{
    //    if (erpRequest != null)
    //    {
    //        var start = int.Parse(erpRequest.Start);
    //        return new SysproRequestModel()
    //        {
    //            action = "SQL",
    //            name = "Online_GetCustomer",
    //            args = new ErpRequestArgs
    //            {
    //                LastChangeDate = erpRequest.DateFrom,
    //                Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber,
    //                PageNumber = start > 0 ? start : 1,
    //                RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100
    //            }
    //        };
    //    }

    //    return new SysproRequestModel();
    //}

    public async Task<dynamic> PrepareErpAccountsRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sysproRequest = new ExpandoObject();
            sysproRequest.action = "SQL";
            sysproRequest.name = "Online_GetCustomer";

            dynamic args = new ExpandoObject();
            args.LastChangeDate = erpRequest.DateFrom;
            args.Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sysproRequest.args = args;

            return sysproRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpPriceSpecialPricingsRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sysproRequest = new ExpandoObject();
            sysproRequest.action = "SQL";
            sysproRequest.name = "Online_GetPrice";

            dynamic args = new ExpandoObject();
            args.LastChangeDate = erpRequest.DateFrom;
            args.Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber;
            args.StockCode = string.IsNullOrWhiteSpace(erpRequest.ProductSku) ? null : erpRequest.ProductSku;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sysproRequest.args = args;

            return sysproRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpProductRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sysproRequest = new ExpandoObject();
            sysproRequest.action = "SQL";
            sysproRequest.name = "Online_GetStockData";

            dynamic args = new ExpandoObject();
            args.LastChangeDate = erpRequest.DateFrom;
            args.StockCode = string.IsNullOrWhiteSpace(erpRequest.ProductSku) ? null : erpRequest.ProductSku;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sysproRequest.args = args;

            return sysproRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpShipToAddressRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sysproRequest = new ExpandoObject();
            sysproRequest.action = "SQL";
            sysproRequest.name = "Online_GetCustomerAddr";

            dynamic args = new ExpandoObject();
            args.Customer = string.IsNullOrWhiteSpace(erpRequest.AccountNumber) ? null : erpRequest.AccountNumber;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sysproRequest.args = args;

            return sysproRequest;
        }

        return new ExpandoObject();
    }

    public async Task<dynamic> PrepareErpStockRequestBody(ErpGetRequestModel erpRequest)
    {
        if (erpRequest != null)
        {
            var start = int.Parse(erpRequest.Start);

            dynamic sysproRequest = new ExpandoObject();
            sysproRequest.action = "SQL";
            sysproRequest.name = "Online_GetStockOnHand";

            dynamic args = new ExpandoObject();
            args.LastChangeDate = erpRequest.DateFrom;
            args.StockCode = string.IsNullOrWhiteSpace(erpRequest.ProductSku) ? null : erpRequest.ProductSku;
            args.PageNumber = start > 0 ? start : 1;
            args.RowsPerPage = erpRequest.Limit > 0 ? erpRequest.Limit : 100;

            sysproRequest.args = args;

            return sysproRequest;
        }

        return new ExpandoObject();
    }
}