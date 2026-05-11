using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services;
using schedule_automation_app_client.Services.Dtos;
using Xunit;

namespace schedule_automation_app_client.Tests;

public class ApiServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null
    };

    private static ApiService CreateServiceWithHandler(FakeHttpMessageHandler handler)
    {
        ApiService service = new ApiService();
        HttpClient client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5284"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        FieldInfo? field = typeof(ApiService).GetField(
            "_httpClient",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(service, client);

        return service;
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    [AvaloniaFact]
    public async Task LoadSubjectsAsync_WhenServerReturnsList_ReturnsSubjects()
    {
        Guid id = Guid.NewGuid();
        List<SubjectListItemDto> list = new List<SubjectListItemDto>
        {
            new SubjectListItemDto(id, "Математика", 7, 6.5, 2)
        };

        PlanResponseDto detail = new PlanResponseDto(
            id, "Математика", 6.5, 7, true, "ok",
            new List<OptimizationItemDto>(),
            new List<GradePlanDto>(),
            false, null,
            new List<ComponentResponseDto>
            {
                new ComponentResponseDto("А", 50, 5, 6, false, 0, false, false, 0),
                new ComponentResponseDto("Б", 50, 5, 7, false, 0, false, false, 0)
            });

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(req =>
        {
            string path = req.RequestUri!.AbsolutePath;
            if (path == "/api/subjects")
            {
                return Response(HttpStatusCode.OK, Serialize(list));
            }

            if (path.StartsWith("/api/subjects/"))
            {
                return Response(HttpStatusCode.OK, Serialize(detail));
            }

            return Response(HttpStatusCode.NotFound, string.Empty);
        });

        ApiService service = CreateServiceWithHandler(handler);

        var (subjects, available) = await service.LoadSubjectsAsync();

        Assert.True(available);
        Assert.Single(subjects);
        Assert.Equal("Математика", subjects[0].Name);
        Assert.Equal(2, subjects[0].Formula.Count);
    }

    [AvaloniaFact]
    public async Task LoadSubjectsAsync_WhenServerReturnsEmptyList_ReturnsEmpty()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.OK, "[]"));

        ApiService service = CreateServiceWithHandler(handler);

        var (subjects, available) = await service.LoadSubjectsAsync();

        Assert.True(available);
        Assert.Empty(subjects);
    }

    [AvaloniaFact]
    public async Task LoadSubjectsAsync_WhenServerNotOk_ReturnsServerUnavailable()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.InternalServerError, string.Empty));

        ApiService service = CreateServiceWithHandler(handler);

        var (subjects, available) = await service.LoadSubjectsAsync();

        Assert.False(available);
        Assert.Empty(subjects);
    }

    [AvaloniaFact]
    public async Task LoadSubjectsAsync_WhenExceptionThrown_ReturnsServerUnavailable()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            throw new HttpRequestException("Network down"));

        ApiService service = CreateServiceWithHandler(handler);

        var (subjects, available) = await service.LoadSubjectsAsync();

        Assert.False(available);
        Assert.Empty(subjects);
    }

    [AvaloniaFact]
    public async Task LoadSubjectsAsync_WhenDetailRequestFails_SkipsThatSubject()
    {
        Guid id1 = Guid.NewGuid();
        Guid id2 = Guid.NewGuid();
        List<SubjectListItemDto> list = new List<SubjectListItemDto>
        {
            new SubjectListItemDto(id1, "A", 5, 4, 0),
            new SubjectListItemDto(id2, "B", 6, 5, 0)
        };

        PlanResponseDto detail2 = new PlanResponseDto(
            id2, "B", 5, 6, true, string.Empty,
            new List<OptimizationItemDto>(),
            new List<GradePlanDto>(),
            false, null,
            new List<ComponentResponseDto>());

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(req =>
        {
            string path = req.RequestUri!.AbsolutePath;
            if (path == "/api/subjects")
            {
                return Response(HttpStatusCode.OK, Serialize(list));
            }

            if (path == $"/api/subjects/{id1}")
            {
                return Response(HttpStatusCode.InternalServerError, string.Empty);
            }

            if (path == $"/api/subjects/{id2}")
            {
                return Response(HttpStatusCode.OK, Serialize(detail2));
            }

            return Response(HttpStatusCode.NotFound, string.Empty);
        });

        ApiService service = CreateServiceWithHandler(handler);

        var (subjects, available) = await service.LoadSubjectsAsync();

        Assert.True(available);
        Assert.Single(subjects);
        Assert.Equal("B", subjects[0].Name);
    }

    [AvaloniaFact]
    public async Task CreateSubjectAsync_WhenSuccess_ReturnsPlanAndNoneError()
    {
        Guid id = Guid.NewGuid();
        PlanResponseDto returned = new PlanResponseDto(
            id, "Тест", 5, 7, true, "ok",
            new List<OptimizationItemDto>(),
            new List<GradePlanDto>(),
            false, null, null);

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.OK, Serialize(returned)));

        ApiService service = CreateServiceWithHandler(handler);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 7 };
        subject.Formula.Add(new GradeComponent { Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5 });

        var (result, error) = await service.CreateSubjectAsync(subject);

        Assert.Equal(ApiError.None, error);
        Assert.NotNull(result);
        Assert.Equal("Тест", result!.Name);
    }

    [AvaloniaFact]
    public async Task CreateSubjectAsync_WhenBadRequest_ReturnsValidationFailed()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.BadRequest, string.Empty));

        ApiService service = CreateServiceWithHandler(handler);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 5 };
        var (result, error) = await service.CreateSubjectAsync(subject);

        Assert.Equal(ApiError.ValidationFailed, error);
        Assert.Null(result);
    }

    [AvaloniaFact]
    public async Task CreateSubjectAsync_WhenServerError_ReturnsServerUnavailable()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.InternalServerError, string.Empty));

        ApiService service = CreateServiceWithHandler(handler);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 5 };
        var (result, error) = await service.CreateSubjectAsync(subject);

        Assert.Equal(ApiError.ServerUnavailable, error);
        Assert.Null(result);
    }

    [AvaloniaFact]
    public async Task CreateSubjectAsync_WhenException_ReturnsServerUnavailable()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            throw new HttpRequestException("down"));

        ApiService service = CreateServiceWithHandler(handler);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 5 };
        var (result, error) = await service.CreateSubjectAsync(subject);

        Assert.Equal(ApiError.ServerUnavailable, error);
        Assert.Null(result);
    }

    [AvaloniaFact]
    public async Task UpdateSubjectAsync_WhenSuccess_ReturnsNoneError()
    {
        Guid id = Guid.NewGuid();
        PlanResponseDto returned = new PlanResponseDto(
            id, "Обновлено", 6, 7, true, string.Empty,
            new List<OptimizationItemDto>(),
            new List<GradePlanDto>(),
            false, null, null);

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(req =>
        {
            Assert.Equal(HttpMethod.Put, req.Method);
            return Response(HttpStatusCode.OK, Serialize(returned));
        });

        ApiService service = CreateServiceWithHandler(handler);

        Subject subject = new Subject { Id = id, Name = "Обновлено", TargetGrade = 7 };
        var (result, error) = await service.UpdateSubjectAsync(subject);

        Assert.Equal(ApiError.None, error);
        Assert.NotNull(result);
    }

    [AvaloniaFact]
    public async Task UpdateSubjectAsync_WhenBadRequest_ReturnsValidationFailed()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.BadRequest, string.Empty));

        ApiService service = CreateServiceWithHandler(handler);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 5 };
        var (result, error) = await service.UpdateSubjectAsync(subject);

        Assert.Equal(ApiError.ValidationFailed, error);
    }

    [AvaloniaFact]
    public async Task UpdateSubjectAsync_WhenServerError_ReturnsServerUnavailable()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.ServiceUnavailable, string.Empty));

        ApiService service = CreateServiceWithHandler(handler);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 5 };
        var (result, error) = await service.UpdateSubjectAsync(subject);

        Assert.Equal(ApiError.ServerUnavailable, error);
    }

    [AvaloniaFact]
    public async Task UpdateSubjectAsync_WhenException_ReturnsServerUnavailable()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            throw new HttpRequestException("down"));

        ApiService service = CreateServiceWithHandler(handler);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 5 };
        var (result, error) = await service.UpdateSubjectAsync(subject);

        Assert.Equal(ApiError.ServerUnavailable, error);
    }

    [AvaloniaFact]
    public async Task DeleteSubjectAsync_WhenSuccess_ReturnsTrue()
    {
        Guid id = Guid.NewGuid();
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(req =>
        {
            Assert.Equal(HttpMethod.Delete, req.Method);
            return Response(HttpStatusCode.NoContent, string.Empty);
        });

        ApiService service = CreateServiceWithHandler(handler);

        bool result = await service.DeleteSubjectAsync(id);
        Assert.True(result);
    }

    [AvaloniaFact]
    public async Task DeleteSubjectAsync_WhenServerError_ReturnsFalse()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.InternalServerError, string.Empty));

        ApiService service = CreateServiceWithHandler(handler);

        bool result = await service.DeleteSubjectAsync(Guid.NewGuid());
        Assert.False(result);
    }

    [AvaloniaFact]
    public async Task DeleteSubjectAsync_WhenException_ReturnsFalse()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            throw new HttpRequestException("down"));

        ApiService service = CreateServiceWithHandler(handler);

        bool result = await service.DeleteSubjectAsync(Guid.NewGuid());
        Assert.False(result);
    }

    [AvaloniaFact]
    public async Task CalculatePlanAsync_WhenSuccess_ReturnsPlan()
    {
        Guid id = Guid.NewGuid();
        PlanResponseDto returned = new PlanResponseDto(
            id, "X", 5, 7, true, "rec",
            new List<OptimizationItemDto>(),
            new List<GradePlanDto>(),
            false, null, null);

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.OK, Serialize(returned)));

        ApiService service = CreateServiceWithHandler(handler);

        PlanResponseDto? plan = await service.CalculatePlanAsync(id);

        Assert.NotNull(plan);
        Assert.Equal(id, plan!.Id);
    }

    [AvaloniaFact]
    public async Task CalculatePlanAsync_WhenServerError_ReturnsNull()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            Response(HttpStatusCode.InternalServerError, string.Empty));

        ApiService service = CreateServiceWithHandler(handler);

        PlanResponseDto? plan = await service.CalculatePlanAsync(Guid.NewGuid());
        Assert.Null(plan);
    }

    [AvaloniaFact]
    public async Task CalculatePlanAsync_WhenException_ReturnsNull()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(_ =>
            throw new HttpRequestException("down"));

        ApiService service = CreateServiceWithHandler(handler);

        PlanResponseDto? plan = await service.CalculatePlanAsync(Guid.NewGuid());
        Assert.Null(plan);
    }

    [AvaloniaFact]
    public async Task CreateSubjectAsync_MapsAllComponentFields_Correctly()
    {
        string? capturedBody = null;
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(req =>
        {
            if (req.Content != null)
            {
                capturedBody = req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            PlanResponseDto plan = new PlanResponseDto(
                Guid.NewGuid(), "x", 0, 5, true, string.Empty,
                new List<OptimizationItemDto>(),
                new List<GradePlanDto>(),
                false, null, null);
            return Response(HttpStatusCode.OK, Serialize(plan));
        });

        ApiService service = CreateServiceWithHandler(handler);

        Subject subject = new Subject { Name = "x", TargetGrade = 5 };
        subject.Formula.Add(new GradeComponent
        {
            Name = "А",
            Weight = 60,
            Complexity = 7,
            CurrentGrade = 6,
            IsBlocking = true,
            BlockingMinimum = 3.5,
            IsAutoGrade = true,
            AutoGradeMinScore = 8
        });
        subject.Formula.Add(new GradeComponent
        {
            Name = "Б",
            Weight = 40,
            Complexity = 5,
            CurrentGrade = 5,
            IsBlocking = false,
            BlockingMinimum = 4,
            IsAutoGrade = false,
            AutoGradeMinScore = 7
        });

        await service.CreateSubjectAsync(subject);

        Assert.NotNull(capturedBody);
        Assert.Contains("\"name\"", capturedBody!);
        Assert.Contains("components", capturedBody!);
        Assert.Contains("minimumGrade", capturedBody!);
    }

    [AvaloniaFact]
    public async Task LoadSubjectsAsync_WhenDetailReturnsNullJson_SkipsThatSubject()
    {
        Guid id = Guid.NewGuid();
        List<SubjectListItemDto> list = new List<SubjectListItemDto>
        {
            new SubjectListItemDto(id, "A", 5, 4, 0)
        };

        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(req =>
        {
            string path = req.RequestUri!.AbsolutePath;
            if (path == "/api/subjects")
            {
                return Response(HttpStatusCode.OK, Serialize(list));
            }

            return Response(HttpStatusCode.OK, "null");
        });

        ApiService service = CreateServiceWithHandler(handler);

        var (subjects, available) = await service.LoadSubjectsAsync();

        Assert.True(available);
        Assert.Empty(subjects);
    }

    private static HttpResponseMessage Response(HttpStatusCode statusCode, string content)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }
}

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_responder(request));
    }
}
