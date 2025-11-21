using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Filters;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Services;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Controllers;

[TokenAuthorize]
public class ODataIntegrationController : BasePluginController
{
    #region Fields

    private readonly ID365Service _d365service;

    #endregion

    #region Ctor
    public ODataIntegrationController(ID365Service d365service)
    {
        _d365service = d365service;
    }
    #endregion

    #region TestActions


    [HttpPost]
    public async Task<IActionResult> GetAccounts([FromBody] ErpGetRequestModel erpRequest)
     {
        var result = await _d365service.GetCustomersFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return Ok(new { Message = "No accounts found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] ErpCreateAccountModel erpRequest)
    {
        var result = await _d365service.CreateAccountNoErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return Ok(new { Message = "No accounts found or service returned no data." });
        }

        if (result.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErrorShortMessage}. Full message: {result.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetProducts([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _d365service.GetProductsFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return Ok(new { Message = "No products found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] ErpPlaceOrderDataModel orderData)
    {
        var result = await _d365service.CreateOrderAsync(orderData);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return Ok(new { Message = "Creating Order Failed." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetOrders([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _d365service.GetOrdersByAccountFromErpAsync(erpRequest);

        if (result == null)
        {
            return Ok(new { Message = "No orders found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetStocks([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _d365service.GetStocksFromErpAsync(erpRequest);

        if (result == null)
        {
            return Ok(new { Message = "No stock data found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetInvoices([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _d365service.GetInvoiceByAccountNoFromErpAsync(erpRequest);

        if (result == null)
        {
            return Ok(new { Message = "No invoice found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }
    [HttpPost]
    public async Task<IActionResult> GetInvoicePDF([FromBody] ErpGetRequestModel erpRequest)
    {
        if (string.IsNullOrWhiteSpace(erpRequest.DocumentNumber))
        {
            return Ok(new { Message = "Please provide a invoice number" });
        }
        var result = await _d365service.GetInvoicePdfByteCodeByDocumentNoFromErpAsync(erpRequest);

        if (result == null)
        {
            return Ok(new { Message = "No invoice found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }
    [HttpPost]

    public async Task<IActionResult> GetShipToAddresses([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _d365service.GetShipToAddressFromErpAsync(erpRequest);

        if (result == null)
        {
            return Ok(new { Message = "No ship to address found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    public async Task<IActionResult> GetSpecialPricing([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _d365service.GetPerAccountProductPricingFromErpAsync(erpRequest);

        if (result == null)
        {
            return Ok(new { Message = "No special pricing found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    public async Task<IActionResult> GetGroupPricing([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _d365service.GetProductGroupPricingFromErpAsync(erpRequest);

        if (result == null)
        {
            return Ok(new { Message = "No group pricing found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                              new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}" });
        }

        return Ok(result);
    }

    #endregion
}