namespace MobileApp.Api.Models;

public class Area
{
    public int AreaId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public int PartId { get; set; }
    public DateTime CreatedDate { get; set; }
}
