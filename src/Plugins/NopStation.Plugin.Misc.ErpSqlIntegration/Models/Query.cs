using System.Xml.Serialization;

namespace NopStation.Plugin.Misc.ErpSqlIntegration.Models;
public class Query
{
    [XmlElement(ElementName = "Document")]
    public Document Document { get; set; }
}
