using System.Data;
using Microsoft.Data.SqlClient;
using MobileApp.Api.Models;

namespace MobileApp.Api.Services;

public class SurveyDataService
{
    private readonly string _connectionString;

    public SurveyDataService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Ward>> GetWardsAsync()
    {
        var wards = new List<Ward>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("SELECT WardId, WardName, CreatedDate FROM Ward ORDER BY WardName", connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        wards.Add(new Ward
                        {
                            WardId = (int)reader["WardId"],
                            WardName = reader["WardName"].ToString() ?? string.Empty,
                            CreatedDate = (DateTime)reader["CreatedDate"]
                        });
                    }
                }
            }
        }
        return wards;
    }

    public async Task<List<Part>> GetPartsByWardIdAsync(int wardId)
    {
        var parts = new List<Part>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("SELECT PartId, PartNumber, WardId, CreatedDate FROM Part WHERE WardId = @wardId ORDER BY PartNumber", connection))
            {
                command.Parameters.AddWithValue("@wardId", wardId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        parts.Add(new Part
                        {
                            PartId = (int)reader["PartId"],
                            PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                            WardId = (int)reader["WardId"],
                            CreatedDate = (DateTime)reader["CreatedDate"]
                        });
                    }
                }
            }
        }
        return parts;
    }

    public async Task<List<Area>> GetAreasByPartIdAsync(int partId)
    {
        var areas = new List<Area>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("SELECT AreaId, AreaName, PartId, CreatedDate FROM Area WHERE PartId = @partId ORDER BY AreaName", connection))
            {
                command.Parameters.AddWithValue("@partId", partId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        areas.Add(new Area
                        {
                            AreaId = (int)reader["AreaId"],
                            AreaName = reader["AreaName"].ToString() ?? string.Empty,
                            PartId = (int)reader["PartId"],
                            CreatedDate = (DateTime)reader["CreatedDate"]
                        });
                    }
                }
            }
        }
        return areas;
    }

    public async Task<List<Street>> GetStreetsByAreaIdAsync(int areaId)
    {
        var streets = new List<Street>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("SELECT StreetId, StreetName, AreaId, CreatedDate FROM Street WHERE AreaId = @areaId ORDER BY StreetName", connection))
            {
                command.Parameters.AddWithValue("@areaId", areaId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        streets.Add(new Street
                        {
                            StreetId = (int)reader["StreetId"],
                            StreetName = reader["StreetName"].ToString() ?? string.Empty,
                            AreaId = (int)reader["AreaId"],
                            CreatedDate = (DateTime)reader["CreatedDate"]
                        });
                    }
                }
            }
        }
        return streets;
    }

    public async Task<int> CreateSurveyAsync(CreateSurveyRequest request)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Convert responses to JSON
            var surveyDataJson = System.Text.Json.JsonSerializer.Serialize(request.Responses);

            using (var command = new SqlCommand(
                "INSERT INTO Survey (WardId, PartId, AreaId, StreetId, SurveyData) VALUES (@wardId, @partId, @areaId, @streetId, @surveyData); SELECT SCOPE_IDENTITY();",
                connection))
            {
                command.Parameters.AddWithValue("@wardId", request.WardId);
                command.Parameters.AddWithValue("@partId", request.PartId);
                command.Parameters.AddWithValue("@areaId", request.AreaId);
                command.Parameters.AddWithValue("@streetId", request.StreetId);
                command.Parameters.AddWithValue("@surveyData", surveyDataJson);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }
    }

    public async Task<List<Survey>> GetAllSurveysAsync()
    {
        var surveys = new List<Survey>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(
                "SELECT SurveyId, WardId, PartId, AreaId, StreetId, SurveyData, CreatedDate FROM Survey ORDER BY CreatedDate DESC",
                connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        surveys.Add(new Survey
                        {
                            SurveyId = (int)reader["SurveyId"],
                            WardId = (int)reader["WardId"],
                            PartId = (int)reader["PartId"],
                            AreaId = (int)reader["AreaId"],
                            StreetId = (int)reader["StreetId"],
                            SurveyData = reader["SurveyData"].ToString() ?? string.Empty,
                            CreatedDate = (DateTime)reader["CreatedDate"]
                        });
                    }
                }
            }
        }
        return surveys;
    }

    // Questionnaire methods
    public async Task<List<QuestionnaireDto>> GetQuestionnairesAsync()
    {
        var questionnaires = new List<QuestionnaireDto>();
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand(
                @"SELECT q.QuestionnaireId, q.QuestionText, q.QuestionType, q.DisplayOrder,
                         o.OptionId, o.OptionText, o.OptionValue, o.DisplayOrder as OptionOrder
                  FROM Questionnaire q
                  LEFT JOIN QuestionnaireOption o ON q.QuestionnaireId = o.QuestionnaireId
                  WHERE q.IsActive = 1
                  ORDER BY q.DisplayOrder, o.DisplayOrder",
                connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var questionMap = new Dictionary<int, QuestionnaireDto>();

                    while (await reader.ReadAsync())
                    {
                        int questionId = (int)reader["QuestionnaireId"];

                        if (!questionMap.ContainsKey(questionId))
                        {
                            questionMap[questionId] = new QuestionnaireDto
                            {
                                QuestionnaireId = questionId,
                                QuestionText = reader["QuestionText"].ToString() ?? string.Empty,
                                QuestionType = reader["QuestionType"].ToString() ?? string.Empty,
                                DisplayOrder = (int)reader["DisplayOrder"],
                                Options = new List<QuestionnaireOptionDto>()
                            };
                        }

                        if (reader["OptionId"] != DBNull.Value)
                        {
                            questionMap[questionId].Options.Add(new QuestionnaireOptionDto
                            {
                                OptionId = (int)reader["OptionId"],
                                OptionText = reader["OptionText"].ToString() ?? string.Empty,
                                OptionValue = reader["OptionValue"].ToString() ?? string.Empty
                            });
                        }
                    }

                    questionnaires.AddRange(questionMap.Values.OrderBy(q => q.DisplayOrder));
                }
            }
        }
        return questionnaires;
    }
}
