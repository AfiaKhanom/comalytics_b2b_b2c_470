using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PeachPayments.Components;

[ViewComponent(Name = "PaymentInfo")]
public class PeachPaymentsViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View("~/Plugins/Payments.PeachPayments/Views/PaymentInfo.cshtml");
    }
}
