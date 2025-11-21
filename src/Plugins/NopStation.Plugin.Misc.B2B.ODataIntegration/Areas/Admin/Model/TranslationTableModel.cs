namespace NopStation.Plugin.Misc.B2B.ODataIntegration.Areas.Admin.Model;

public record TranslationTableModel
{
    public List<KeyValueItem> OrderTypes { get; set; }
    public List<KeyValueItem> DeliveryMethods { get; set; }
    public List<KeyValueItem> DocumentTypes { get; set; }
}

public record KeyValueItem
{
    public string Key { get; set; }
    public string Value { get; set; }
}

