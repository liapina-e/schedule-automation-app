using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public async Task<ObservableCollection<Subject>> LoadSubjectsAsync()
    {
        try
        {
            HttpResponseMessage listResponse = await _httpClient.GetAsync("/api/subjects");

            if (!listResponse.IsSuccessStatusCode)
            {
                return new ObservableCollection<Subject>();
            }

            string listJson = await listResponse.Content.ReadAsStringAsync();
            List<SubjectListItemDto>? items = JsonSerializer.Deserialize<List<SubjectListItemDto>>(listJson, _jsonOptions);

            if (items == null || items.Count == 0)
            {
                return new ObservableCollection<Subject>();
            }

            ObservableCollection<Subject> result = new ObservableCollection<Subject>();

            foreach (SubjectListItemDto item in items)
            {
                HttpResponseMessage detailResponse = await _httpClient.GetAsync($"/api/subjects/{item.Id}");

                if (!detailResponse.IsSuccessStatusCode)
                {
                    continue;
                }

                string detailJson = await detailResponse.Content.ReadAsStringAsync();
                PlanResponseDto? detail = JsonSerializer.Deserialize<PlanResponseDto>(detailJson, _jsonOptions);

                if (detail == null)
                {
                    continue;
                }

                result.Add(MapToSubject(detail));
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке предметов: {ex.Message}");
            return new ObservableCollection<Subject>();
        }
    }

    public async Task<PlanResponseDto?> CreateSubjectAsync(Subject subject)
    {
        try
        {
            CreateSubjectRequestDto request = MapToRequest(subject);
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/subjects", request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PlanResponseDto>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании предмета: {ex.Message}");
            return null;
        }
    }

    public async Task<PlanResponseDto?> UpdateSubjectAsync(Subject subject)
    {
        try
        {
            CreateSubjectRequestDto request = MapToRequest(subject);
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"/api/subjects/{subject.Id}", request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PlanResponseDto>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении предмета: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteSubjectAsync(Guid id)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/subjects/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении предмета: {ex.Message}");
            return false;
        }
    }

    public async Task<PlanResponseDto?> CalculatePlanAsync(Guid id)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/subjects/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PlanResponseDto>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при расчёте плана: {ex.Message}");
            return null;
        }
    }

    private static CreateSubjectRequestDto MapToRequest(Subject subject)
    {
        return new CreateSubjectRequestDto(
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
                AutoGradeMinScore: c.IsAutoGrade ? c.AutoGradeMinScore : 0.0
            )).ToList()
        );
    }

    private static Subject MapToSubject(PlanResponseDto dto)
    {
        Subject subject = new Subject
        {
            Id = dto.Id,
            Name = dto.Name,
            TargetGrade = dto.TargetGrade
        };

        if (dto.Components != null)
        {
            foreach (ComponentResponseDto c in dto.Components)
            {
                subject.Formula.Add(new GradeComponent
                {
                    Name = c.Name,
                    Weight = c.Weight,
                    Complexity = c.Complexity,
                    CurrentGrade = c.CurrentGrade,
                    IsBlocking = c.IsBlocking,
                    BlockingMinimum = c.MinimumGrade,
                    IsGraded = c.IsGraded,
                    IsAutoGrade = c.IsAutoGrade,
                    AutoGradeMinScore = c.AutoGradeMinScore
                });
            }
        }

        return subject;
    }
}