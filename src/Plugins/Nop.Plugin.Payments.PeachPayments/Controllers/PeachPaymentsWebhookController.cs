using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.PeachPayments.Controllers;

public class PeachPaymentsWebhookController : Controller
{
    #region Fields

    private readonly PeachPaymentsSettings _settings;
    private readonly ILogger _logger;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IOrderService _orderService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly ICurrencyService _currencyService;

    #endregion

    #region Ctor

    public PeachPaymentsWebhookController(ILogger logger,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        ICurrencyService currencyService,
        IPaymentPluginManager paymentPluginManager,
        PeachPaymentsSettings settings)
    {
        _logger = logger;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _paymentPluginManager = paymentPluginManager;
        _settings = settings;
        _currencyService = currencyService;

    }

    #endregion

    #region Methods

    public async Task<IActionResult> Result()
    {
        if (!(await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.PeachPayments") is PeachPaymentsPaymentMethod processor) ||
             !_paymentPluginManager.IsPluginActive(processor) ||
             !processor.PluginDescriptor.Installed)
            throw new NopException("Peach Payments module cannot be loaded");

        try
        {
            var merchant_id = string.Empty;
            if (Request.Form["merchantTransactionId"].Count == 1)
                merchant_id = Request.Form["merchantTransactionId"];

            var order_id = merchant_id.Replace(_settings.MerchantIdPrefix, string.Empty);

            await Callback();
            if (!VerifySignaturePeach())
            {
                await _logger.ErrorAsync($"Error while verifying signature for request with order id: {order_id}", new NopException("Peach Payments module cannot verify the signature"));
                throw new NopException("Peach Payments module cannot verify the signature");
            }

            var payment_id = string.Empty;
            if (Request.Form["id"].Count == 1)
                payment_id = Request.Form["id"];

            var payment_status = GetPaymentStatusFromResultPeach();

            if (payment_status == "COMPLETE")
            {
                return RedirectToRoute("CheckoutCompleted", new { orderId = order_id });
            }
            else
            {
                return RedirectToRoute("ShoppingCart");
            }
        }
        catch(Exception e)
        {
            await _logger.ErrorAsync("Exception while reading request", e);            
        }

        return RedirectToRoute("ShoppingCart");
    }

    public string GetPaymentStatusFromResultPeach()
    {
        var payment_status = string.Empty;
        if (Request.Form["result.code"].Count == 1)
        {
            switch (Request.Form["result.code"])
            {
                case "000.000.000":
                    payment_status = "COMPLETE";
                    break;

                case "000.100.110":
                    payment_status = "COMPLETE";
                    break;
                case "600.200.500":
                case "200.300.404":
                case "600.200.400":
                case "800.100.152":
                case "800.900.300":
                case "800.900.201":
                    payment_status = "FAILED";
                    break;

                case "000.200.100":
                case "000.200.000":
                    payment_status = "PENDING";
                    break;
                case "100.396.101":
                case "100.396.104":
                    payment_status = "CANCELLED";
                    break;
                default:
                    // If unknown status, do nothing (safest course of action)
                    break;
            }
        }
        return payment_status;
    }

    public async Task<IActionResult> Callback()
    {
        try
        { 
            if (Request.ContentLength != null && Request.ContentType != null && Request.Form.Count>0)
            {
                if (!(await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.PeachPayments") is PeachPaymentsPaymentMethod processor) ||
                    !_paymentPluginManager.IsPluginActive(processor) || 
                    !processor.PluginDescriptor.Installed)
                    throw new NopException("Peach Payments module cannot be loaded");

                var merchant_id = string.Empty;
                if (Request.Form["merchantTransactionId"].Count == 1)
                    merchant_id = Request.Form["merchantTransactionId"];

                var order_id = merchant_id.Replace(_settings.MerchantIdPrefix, string.Empty);

                var payment_id = string.Empty;
                if (Request.Form["id"].Count == 1)
                    payment_id = Request.Form["id"];

                var payment_status = string.Empty;
                if (!VerifySignaturePeach())
                {
                    await _logger.ErrorAsync($"Error while verifying signature for request with order id: {order_id}", new NopException());
                    return Ok();
                }

                if (!IsIpValid(Request.Host.ToString()))
                {
                    await _logger.ErrorAsync($"Error while verifying ipaddress for request with order id: {order_id}", new NopException());
                }

                var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(order_id));
                if (order == null)
                {
                    await _logger.ErrorAsync($"Error while verifying order for request with order id: {order_id}", new NopException());
                    return Ok();
                }

                var amount_converted = decimal.Zero;

                var peachcurrencycode = await _currencyService.GetCurrencyByIdAsync(_settings.CurrencyId);
                if (order.CustomerCurrencyCode == peachcurrencycode.CurrencyCode)
                {
                    amount_converted = order.OrderTotal;
                }
                else
                {
                    var primary_currency = await _currencyService.GetCurrencyByCodeAsync(order.CustomerCurrencyCode);
                    var amount_to_be_converted = order.OrderTotal;
                    amount_converted = await _currencyService.ConvertCurrencyAsync(amount_to_be_converted, primary_currency, peachcurrencycode);
                }

                if (!VerifyAmount(amount_converted, decimal.Parse(Request.Form["amount"])))
                {
                    await _logger.ErrorAsync($"Error while verifying amount for request with order id: {order_id}", new NopException());
                }

                if (Request.Form["result.code"].Count == 1)
                {
                    payment_status = GetPaymentStatusFromResultPeach();
                }
                else
                {
                    await _logger.ErrorAsync($"Error while verifying result code for  request with order id: {order_id}", new NopException());
                    return Ok();
                }

                if (payment_status == "COMPLETE")
                {
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);

                            if (order.OrderStatus != OrderStatus.Processing)
                            {
                                order.OrderStatusId = (int)OrderStatus.Processing;
                                order.OrderStatus = OrderStatus.Processing; 
                            }
                        await _orderService.UpdateOrderAsync(order);
                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = order.Id,
                            Note = $"Payment Status changed to {PaymentStatus.Paid} and order status changed to {OrderStatus.Processing}",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        await _logger.InsertLogAsync(LogLevel.Information, $"Order Payment status changed to {order.PaymentStatus} for order id: {order_id}");
                    }
                    else
                    {
                        await _logger.InsertLogAsync(LogLevel.Information, $"Order Payment status already same {order.PaymentStatus} for order id: {order_id}");
                    }
                }
                else if (payment_status == "FAILED")
                {
                    if (order.PaymentStatus != PaymentStatus.Paid && order.PaymentStatus != PaymentStatus.Pending)
                    {
                        order.PaymentStatus = PaymentStatus.Pending;
                        await _orderService.UpdateOrderAsync(order);
                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = order.Id,
                            Note = $"Payment Status changed to {PaymentStatus.Pending}",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        await _logger.InsertLogAsync(LogLevel.Information, $"Order Payment status changed to {order.PaymentStatus} for order id: {order_id}");
                    }
                    else
                    {
                        await _logger.InsertLogAsync(LogLevel.Information, $"Order Payment status already same {order.PaymentStatus} for order id: {order_id}  or the order was already completed");
                    }
                }
                else if (payment_status == "PENDING")
                {
                    await _logger.InsertLogAsync(LogLevel.Information, $"Order is pending for order id: {order_id}");
                }
                else if (payment_status == "CANCELLED")
                {
                    if (order.PaymentStatus != PaymentStatus.Paid && order.PaymentStatus != PaymentStatus.Pending)
                    {
                        order.PaymentStatus = PaymentStatus.Pending;
                        await _orderService.UpdateOrderAsync(order);
                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = order.Id,
                            Note = $"Payment Status changed to {order.PaymentStatus}",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        await _logger.InsertLogAsync(LogLevel.Information, $"Order Payment status changed to {order.PaymentStatus} for order id: {order_id}");
                    }
                    else
                    {
                        await _logger.InsertLogAsync(LogLevel.Information, $"Order Payment status already same {order.PaymentStatus} for order id: {order_id}  or the order was already completed");
                    }
                }
                else
                {
                    await _logger.ErrorAsync($"Error while fetching result code for order with id {order_id}", new NopException());
                    return Ok();
                }
            }
        }
        catch(Exception e)
        {
            await _logger.ErrorAsync("Exception while reading request", e);
        }

        return Ok();
    }


    public bool IsIpValid(string currentIP)
    {
        var validHosts = new string[] { "testsecure.peachpayments.com", "secure.peachpayments.com" };
        var ipvalid = false;

        foreach (var hostName in validHosts)
        {
            var ips = Dns.GetHostAddresses(hostName);
            foreach (var ip in ips)
            {
                if (currentIP == ip.ToString())
                {
                    ipvalid = true;
                }
            }
        }
        return ipvalid;
    }

    public bool VerifySignaturePeach()
    {
        var signatureString = string.Empty;
        foreach (var key in Request.Form.Keys)
        {
            if (key != "signature")
                signatureString += key + Request.Form[key];
        }
        var keyhmac = string.Empty;
        var enc = Encoding.Default;
        if (_settings.SandBoxModeId == ((int)SandboxMode.Enabled))
            keyhmac = _settings.SecretTokenSandbox;
        else
            keyhmac = _settings.SecretToken;
        var hash = new HMACSHA256(enc.GetBytes(keyhmac));
        var baHashedText = hash.ComputeHash(enc.GetBytes(signatureString));
        var signature = string.Join("", baHashedText.Select(b => b.ToString("x2")));
        return signature == Request.Form["signature"];
    }

    public bool VerifyAmount(decimal amount1, decimal amount2)
    {
        return amount1 == amount2;
    }

    #endregion
}
