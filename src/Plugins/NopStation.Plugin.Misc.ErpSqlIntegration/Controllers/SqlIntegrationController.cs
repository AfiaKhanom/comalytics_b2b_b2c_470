using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.ErpSqlIntegration.Filters;
using NopStation.Plugin.Misc.ErpSqlIntegration.Models;
using NopStation.Plugin.Misc.ErpSqlIntegration.Services;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Controllers;

[TokenAuthorize]
public class SqlIntegrationController : BasePluginController
{

    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IB2BAccountService _erpAccountService;
    private readonly IB2BInvoiceService _b2BInvoiceService;
    private readonly IB2BStockService _b2BStockService;
    private readonly IB2BPricingService _b2BPricingService;
    private readonly IErpOrderService _erpOrderService;
    private readonly IShipToAddressService _shipToAddressService;
    private readonly IB2BProductService _b2BProductService;

    #endregion

    #region Ctor

    public SqlIntegrationController(IStoreContext storeContext,
        ISettingService settingService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IB2BAccountService erpAccountService,
        IB2BInvoiceService b2BInvoiceService,
        IB2BStockService b2BStockService,
        IB2BPricingService b2BPricingService,
        IErpOrderService erpOrderService,
        IShipToAddressService shipToAddressService,
        IB2BProductService b2BProductService)
    {
        _storeContext = storeContext;
        _settingService = settingService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _erpAccountService = erpAccountService;
        _b2BInvoiceService = b2BInvoiceService;
        _b2BStockService = b2BStockService;
        _b2BPricingService = b2BPricingService;
        _erpOrderService = erpOrderService;
        _shipToAddressService = shipToAddressService;
        _b2BProductService = b2BProductService;
    }

    #endregion

    #region TestActions

    [HttpPost]
    public async Task<IActionResult> GetAccounts([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _erpAccountService.GetAccountsFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No accounts found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetShipToAddresses([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _shipToAddressService.GetShipToAddressFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No ship to address found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetOrders([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _erpOrderService.GetOrderByAccountFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No orders found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetStocks([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BStockService.GetStockFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No stock found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetPrices([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BPricingService.GetPerAccountProductPricingFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No pricing found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetGroupPrices([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BPricingService.GetProductGroupPricingFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No Group Price found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetProducts([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BProductService.GetProductsFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No products found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetInvoices([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BInvoiceService.GetInvoiceByAccountNoFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No invoice found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetStatementPdfHex([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BInvoiceService.GetStatementPdfByteCodeFromErpAsync(erpRequest);
        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No invoice found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetInvoicePdfHex([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BInvoiceService.GetInvoicePdfByteCodeByDocumentNoFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No invoice found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ErpResponseModel> CreateOrderOnErpAsync([FromBody] ErpPlaceOrderDataModel erpRequest)
    {
        return await _erpOrderService.CreateOrderOnErpAsync(erpRequest);
    }

    #endregion
}