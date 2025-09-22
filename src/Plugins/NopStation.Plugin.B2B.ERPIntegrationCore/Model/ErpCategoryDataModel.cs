namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpCategoryDataModel
{
    public string Description { get; set; }
    public string CategoryName { get; set; }
    
    // Category path in format "A >> B >> C >> D"
    public string CategoryPath { get; set; }

}
