using Microsoft.Extensions.Logging.Abstractions;
using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Services.Implementation;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Tests;

public class StatsServiceTests
{
    [Fact]
    public async Task GetStatsAsync_EmptySubjects_ReturnsZeroStats()
    {
        FakeSubjectRepository repo = new FakeSubjectRepository(new List<Subject>());
        GradeCalculationService calcService = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        StatsService stats = new StatsService(repo, calcService);

        StatsResponse response = await stats.GetStatsAsync();

        Assert.Equal(0, response.TotalSubjects);
        Assert.Equal(0, response.AverageTargetGrade);
        Assert.Equal(0, response.MostCommonComplexity);
        Assert.Equal(0, response.TotalComponents);
        Assert.Equal(0, response.AverageComponentsPerSubject);
        Assert.Equal(0, response.AchievableSubjects);
    }

    [Fact]
    public async Task GetStatsAsync_OneSubject_ReturnsTotalSubjectsOne()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: ("Экзамен", 100, 5, 8, false, false, 0, false, 0));

        FakeSubjectRepository repo = new FakeSubjectRepository(new List<Subject> { subject });
        GradeCalculationService calcService = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        StatsService stats = new StatsService(repo, calcService);

        StatsResponse response = await stats.GetStatsAsync();

        Assert.Equal(1, response.TotalSubjects);
        Assert.Equal(7.0, response.AverageTargetGrade);
        Assert.Equal(1, response.TotalComponents);
        Assert.Equal(1.0, response.AverageComponentsPerSubject);
    }

    [Fact]
    public async Task GetStatsAsync_AchievableSubject_CountedCorrectly()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 4,
            components: ("Экзамен", 100, 5, 9, false, false, 0, false, 0));

        FakeSubjectRepository repo = new FakeSubjectRepository(new List<Subject> { subject });
        GradeCalculationService calcService = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        StatsService stats = new StatsService(repo, calcService);

        StatsResponse response = await stats.GetStatsAsync();

        Assert.Equal(1, response.AchievableSubjects);
    }

    [Fact]
    public async Task GetStatsAsync_NotAchievableSubject_NotCounted()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 10,
            components: ("Экзамен", 100, 5, 3, true, false, 0, false, 0));

        FakeSubjectRepository repo = new FakeSubjectRepository(new List<Subject> { subject });
        GradeCalculationService calcService = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        StatsService stats = new StatsService(repo, calcService);

        StatsResponse response = await stats.GetStatsAsync();

        Assert.Equal(0, response.AchievableSubjects);
    }

    [Fact]
    public async Task GetStatsAsync_MostCommonComplexity_ReturnsMode()
    {
        Subject subject = TestHelpers.CreateSubject(
            targetGrade: 7,
            components: [
                ("А", 30, 3, 5, false, false, 0, false, 0),
                ("Б", 40, 3, 5, false, false, 0, false, 0),
                ("В", 30, 7, 5, false, false, 0, false, 0)
            ]);

        FakeSubjectRepository repo = new FakeSubjectRepository(new List<Subject> { subject });
        GradeCalculationService calcService = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        StatsService stats = new StatsService(repo, calcService);

        StatsResponse response = await stats.GetStatsAsync();

        Assert.Equal(3, response.MostCommonComplexity);
    }

    [Fact]
    public async Task GetStatsAsync_TwoSubjects_AverageGradeCorrect()
    {
        Subject s1 = TestHelpers.CreateSubject(targetGrade: 6, components: ("А", 100, 5, 5, false, false, 0, false, 0));
        Subject s2 = TestHelpers.CreateSubject(targetGrade: 8, components: ("Б", 100, 5, 5, false, false, 0, false, 0));

        FakeSubjectRepository repo = new FakeSubjectRepository(new List<Subject> { s1, s2 });
        GradeCalculationService calcService = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        StatsService stats = new StatsService(repo, calcService);

        StatsResponse response = await stats.GetStatsAsync();

        Assert.Equal(7.0, response.AverageTargetGrade);
    }

    [Fact]
    public async Task GetStatsAsync_TwoSubjects_TotalComponentsCorrect()
    {
        Subject s1 = TestHelpers.CreateSubject(targetGrade: 6,
            components: [
                ("А", 50, 5, 5, false, false, 0, false, 0),
                ("Б", 50, 5, 5, false, false, 0, false, 0)
            ]);
        Subject s2 = TestHelpers.CreateSubject(targetGrade: 7, components: ("В", 100, 5, 5, false, false, 0, false, 0));

        FakeSubjectRepository repo = new FakeSubjectRepository(new List<Subject> { s1, s2 });
        GradeCalculationService calcService = new GradeCalculationService(NullLogger<GradeCalculationService>.Instance);
        StatsService stats = new StatsService(repo, calcService);

        StatsResponse response = await stats.GetStatsAsync();

        Assert.Equal(3, response.TotalComponents);
        Assert.Equal(1.5, response.AverageComponentsPerSubject);
    }
}