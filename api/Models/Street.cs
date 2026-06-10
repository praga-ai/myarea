namespace MobileApp.Api.Models;

public class Street
{
    public int StreetId { get; set; }
    public string StreetName { get; set; } = string.Empty;
    public int AreaId { get; set; }
    public DateTime CreatedDate { get; set; }
}
