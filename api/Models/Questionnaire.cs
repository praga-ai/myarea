namespace MobileApp.Api.Models;

public class QuestionnaireDto
{
    public int QuestionnaireId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<QuestionnaireOptionDto> Options { get; set; } = new();
}

public class QuestionnaireOptionDto
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string OptionValue { get; set; } = string.Empty;
}

public class SurveyResponseRequest
{
    public int WardId { get; set; }
    public int PartId { get; set; }
    public int AreaId { get; set; }
    public int StreetId { get; set; }
    public Dictionary<int, string> Responses { get; set; } = new(); // QuestionnaireId -> SelectedValue
}
