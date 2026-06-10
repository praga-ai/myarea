namespace MobileApp.Api.Models;

public class Survey
{
    public int SurveyId { get; set; }
    public int WardId { get; set; }
    public int PartId { get; set; }
    public int AreaId { get; set; }
    public int StreetId { get; set; }
    public string SurveyData { get; set; } = string.Empty; // JSON format
    public DateTime CreatedDate { get; set; }
}

public class CreateSurveyRequest
{
    public int WardId { get; set; }
    public int PartId { get; set; }
    public int AreaId { get; set; }
    public int StreetId { get; set; }
    public Dictionary<int, string> Responses { get; set; } = new(); // QuestionnaireId -> SelectedValue
}
