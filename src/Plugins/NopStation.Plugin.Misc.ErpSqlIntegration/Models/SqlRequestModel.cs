using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Models;

public class ErpRequestArgs
{
    public DateTime? LastChangeDate { get; set; }
    public string Customer { get; set; }
    public int PageNumber { get; set; }
    public int RowsPerPage { get; set; }
}

public class SqlRequestModel
{
    public string action { get; set; }
    public string name { get; set; }
     
    [JsonProperty(PropertyName = "operator")]
    public string Operator { get; set; }
    public string operatorPassword { get; set; }
    public string company { get; set; }
    public string companyPassword { get; set; }
    public string businessObject { get; set; }
    public string method { get; set; }
    public string xmlIn { get; set; }
    public string xmlParameters { get; set; }
     
}