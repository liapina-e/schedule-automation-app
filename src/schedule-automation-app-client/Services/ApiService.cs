using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services.Dtos;

namespace schedule_automation_app_client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(AppSettings.ServerBaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<PlanResponseDto?> CalculatePlanAsync(Subject subject)
    {
        try
        {
            SubjectRequestDto request = new SubjectRequestDto(
                    Name: subject.Name,
                    TargetGrade: subject.TargetGrade,
                    Components: subject.Formula.Select(c => new ComponentRequestDto(
                        Name: c.Name,
                        Weight: c.Weight,
                        Complexity: c.Complexity,
                        CurrentGrade: c.CurrentGrade,
                        IsBlocking: c.IsBlocking,
                        MinimumGrade: c.IsBlocking ? c.BlockingMinimum : 0.0,
                        IsGraded: c.IsGraded,
                        IsAutoGrade: c.IsAutoGrade,
                        AutoGradeMinScore: c.AutoGradeMinScore
                    )).ToList()
                );

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/subjects", request);

            string json = await response.Content.ReadAsStringAsync();
            PlanResponseDto? result = JsonSerializer.Deserialize<PlanResponseDto>(json, _jsonOptions);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при запросе к серверу: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> IsServerAvailableAsync()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("/api/subjects");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}