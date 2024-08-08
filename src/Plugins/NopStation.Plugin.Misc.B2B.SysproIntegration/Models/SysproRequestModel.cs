namespace NopStation.Plugin.Misc.B2B.SysproIntegration.Models;

public class ErpRequestArgs
{
    public DateTime? LastChangeDate { get; set; }
    public string Customer { get; set; }
    public int PageNumber { get; set; }
    public int RowsPerPage { get; set; }
}

public class SysproRequestModel
{
    public string action { get; set; }
    public string name { get; set; }
    public ErpRequestArgs args { get; set; }
}