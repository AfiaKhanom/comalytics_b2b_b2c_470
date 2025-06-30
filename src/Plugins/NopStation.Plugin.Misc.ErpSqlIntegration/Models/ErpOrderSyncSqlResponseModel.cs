namespace NopStation.Plugin.Misc.ErpSqlIntegration.Models;
public class ErpOrderSyncSqlResponseModel
{
    public string Location { get; set; }
    public decimal Quantity { get; set; }
    public string OrderUom { get; set; }
    public decimal UnitPriceExclTax { get; set; }
    public decimal UnitPriceIncl { get; set; }
    public decimal LineTotalExcl { get; set; }
    public decimal LineTotalIncl { get; set; }
    public string Description { get; set; }
    public string Sku { get; set; }
    public DateTime DateRequired { get; set; }
    public string DeliveryMethod { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string SpecInstruct { get; set; }
    public string AccountNumber { get; set; }
    public string AccountName { get; set; }
    public string OrderNumber { get; set; }
    public string CustomerReference { get; set; }
    public string EcomOrderNumber { get; set; }
    public string OrderType { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalExcl { get; set; }
    public decimal VAT { get; set; }
    public decimal TotalIncl { get; set; }
    public string DeliveryInstruction { get; set; }

    //shipping info
    public string ShippingName { get; set; }
    public string ShippingCompany { get; set; }
    public string ShippingPhone { get; set; }
    public string ShippingEmail { get; set; }
    public string ShippingAddress1 { get; set; }
    public string ShippingAddress2 { get; set; }
    public string ShippingCity { get; set; }
    public string ShippingPostalCode { get; set; }
    public string ShippingCountryCode { get; set; }
    public string ShippingProvince { get; set; }

    //billing info
    public string BillingName { get; set; }
    public string BillingCompany { get; set; }
    public string BillingPhone { get; set; }
    public string BillingEmail { get; set; }
    public string BillingAddress1 { get; set; }
    public string BillingAddress2 { get; set; }
    public string BillingCity { get; set; }
    public string BillingPostalCode { get; set; }
    public string BillingCountryCode { get; set; }
    public string BillingProvince { get; set; }

    public string CustomerName { get; set; }
    public string CustomerFirstName { get; set; }
    public string CustomerLastName { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public string CustomerMobileNumber { get; set; }
    public string CustomerEmail { get; set; }
}
