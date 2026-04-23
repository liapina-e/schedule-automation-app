using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services.Dtos;

namespace schedule_automation_app_client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5000";

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
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
                    IsBlocking: false,
                    MinimumGrade: 0
                )).ToList()
            );

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/subjects", request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<PlanResponseDto>();
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
            var response = await _httpClient.GetAsync("/api/subjects");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}