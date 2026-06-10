namespace MobileApp.Api.Models;

public class Part
{
    public int PartId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public int WardId { get; set; }
    public DateTime CreatedDate { get; set; }
}
