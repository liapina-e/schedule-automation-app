using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Moq;
using schedule_automation_app_client.Models;
using schedule_automation_app_client.Services;
using schedule_automation_app_client.Services.Dtos;
using schedule_automation_app_client.ViewModels;
using Xunit;

namespace schedule_automation_app_client.Tests;

public class MainViewModelApiTests
{
    private static void InjectApiService(MainViewModel vm, IApiService mock)
    {
        FieldInfo? field = typeof(MainViewModel).GetField(
            "_apiService",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(vm, mock);
    }

    private static void MarkSynced(MainViewModel vm, Guid id)
    {
        FieldInfo? field = typeof(MainViewModel).GetField(
            "_syncedIds",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        HashSet<Guid> set = (HashSet<Guid>)field!.GetValue(vm)!;
        set.Add(id);
    }

    private static MainViewModel CreateVmWithMock(out Mock<IApiService> mock)
    {
        mock = new Mock<IApiService>();
        mock.Setup(m => m.LoadSubjectsAsync())
            .ReturnsAsync((new ObservableCollection<Subject>(), true));

        MainViewModel vm = new MainViewModel();
        InjectApiService(vm, mock.Object);
        return vm;
    }

    private static async Task WaitForLoadingCycle(MainViewModel vm, int timeoutMs = 5000)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            if (!vm.IsLoading && vm.StatusMessage != "Считаем план..." && vm.StatusMessage != "Обновляем предмет...")
            {
                return;
            }

            await Task.Delay(10);
        }
    }

    private static Subject BuildValidSubject()
    {
        Subject subject = new Subject { Name = "Тест", TargetGrade = 7 };
        subject.Formula.Add(new GradeComponent
        {
            Name = "А", Weight = 100, Complexity = 5, CurrentGrade = 5
        });
        return subject;
    }

    [AvaloniaFact]
    public async Task ExecuteCalculatePlan_WhenSubjectNotSyncedAndCreateSucceeds_SetsCurrentPlan()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();
        Guid serverId = Guid.NewGuid();

        PlanResponseDto createResponse = new PlanResponseDto(
            serverId, "Тест", 5, 7, true, "ok",
            new List<OptimizationItemDto>
            {
                new OptimizationItemDto(Guid.NewGuid(), "А", 5, 7, 1, "r")
            },
            new List<GradePlanDto>(),
            false, null, null);

        mock.Setup(m => m.CreateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync((createResponse, ApiError.None));

        mock.Setup(m => m.CalculatePlanAsync(serverId))
            .ReturnsAsync(createResponse);

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        Assert.True(vm.CanCalculatePlan);

        vm.CalculatePlanCommand.Execute(null);
        await WaitForLoadingCycle(vm);

        Assert.NotNull(vm.CurrentPlan);
        Assert.False(vm.IsLoading);
        Assert.Contains("план", vm.StatusMessage.ToLowerInvariant());
    }

    [AvaloniaFact]
    public async Task ExecuteCalculatePlan_WhenCreateValidationFails_SetsErrorStatus()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();

        mock.Setup(m => m.CreateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync(((PlanResponseDto?)null, ApiError.ValidationFailed));

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        vm.CalculatePlanCommand.Execute(null);
        await WaitForLoadingCycle(vm);

        Assert.False(vm.IsLoading);
        Assert.Equal("Ошибка в данных формулы.", vm.StatusMessage);
    }

    [AvaloniaFact]
    public async Task ExecuteCalculatePlan_WhenCreateServerUnavailable_SetsErrorStatus()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();

        mock.Setup(m => m.CreateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync(((PlanResponseDto?)null, ApiError.ServerUnavailable));

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        vm.CalculatePlanCommand.Execute(null);
        await WaitForLoadingCycle(vm);

        Assert.False(vm.IsLoading);
        Assert.Contains("сервер", vm.StatusMessage.ToLowerInvariant());
    }

    [AvaloniaFact]
    public async Task ExecuteCalculatePlan_WhenSubjectAlreadySynced_CallsUpdate()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();
        MarkSynced(vm, subject.Id);

        PlanResponseDto plan = new PlanResponseDto(
            subject.Id, "Тест", 5, 7, true, "ok",
            new List<OptimizationItemDto>
            {
                new OptimizationItemDto(Guid.NewGuid(), "А", 5, 7, 1, "r")
            },
            new List<GradePlanDto>(),
            false, null, null);

        mock.Setup(m => m.UpdateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync((plan, ApiError.None));

        mock.Setup(m => m.CalculatePlanAsync(subject.Id))
            .ReturnsAsync(plan);

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        vm.CalculatePlanCommand.Execute(null);
        await WaitForLoadingCycle(vm);

        mock.Verify(m => m.UpdateSubjectAsync(It.IsAny<Subject>()), Times.AtLeastOnce);
        Assert.NotNull(vm.CurrentPlan);
    }

    [AvaloniaFact]
    public async Task ExecuteCalculatePlan_WhenUpdateValidationFails_SetsErrorStatus()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();
        MarkSynced(vm, subject.Id);

        mock.Setup(m => m.UpdateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync(((PlanResponseDto?)null, ApiError.ValidationFailed));

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        vm.CalculatePlanCommand.Execute(null);
        await WaitForLoadingCycle(vm);

        Assert.False(vm.IsLoading);
        Assert.Equal("Ошибка в данных формулы.", vm.StatusMessage);
    }

    [AvaloniaFact]
    public async Task ExecuteCalculatePlan_WhenUpdateServerUnavailable_SetsErrorStatus()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();
        MarkSynced(vm, subject.Id);

        mock.Setup(m => m.UpdateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync(((PlanResponseDto?)null, ApiError.ServerUnavailable));

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        vm.CalculatePlanCommand.Execute(null);
        await WaitForLoadingCycle(vm);

        Assert.False(vm.IsLoading);
        Assert.Contains("сервер", vm.StatusMessage.ToLowerInvariant());
    }

    [AvaloniaFact]
    public async Task ExecuteCalculatePlan_WhenCalculateReturnsNull_SetsServerUnavailableMessage()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();
        MarkSynced(vm, subject.Id);

        mock.Setup(m => m.UpdateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync((new PlanResponseDto(
                subject.Id, "x", 5, 7, true, string.Empty,
                new List<OptimizationItemDto>(),
                new List<GradePlanDto>(),
                false, null, null), ApiError.None));

        mock.Setup(m => m.CalculatePlanAsync(subject.Id))
            .ReturnsAsync((PlanResponseDto?)null);

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        vm.CalculatePlanCommand.Execute(null);
        await WaitForLoadingCycle(vm);

        Assert.False(vm.IsLoading);
        Assert.Null(vm.CurrentPlan);
        Assert.Contains("сервер", vm.StatusMessage.ToLowerInvariant());
    }

    [AvaloniaFact]
    public async Task ExecuteCalculatePlan_WhenPlanNotAchievable_SetsUnreachableMessage()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();
        MarkSynced(vm, subject.Id);

        PlanResponseDto plan = new PlanResponseDto(
            subject.Id, "Тест", 5, 7, false, "Недостижимо",
            new List<OptimizationItemDto>(),
            new List<GradePlanDto>(),
            false, null, null);

        mock.Setup(m => m.UpdateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync((plan, ApiError.None));

        mock.Setup(m => m.CalculatePlanAsync(subject.Id))
            .ReturnsAsync(plan);

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        vm.CalculatePlanCommand.Execute(null);
        await WaitForLoadingCycle(vm);

        Assert.False(vm.IsLoading);
        Assert.Contains("недостижим", vm.StatusMessage.ToLowerInvariant());
    }

    [AvaloniaFact]
    public async Task ExecuteCalculatePlan_WhenPlanIsEmpty_SetsGoalAchievedMessage()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();
        MarkSynced(vm, subject.Id);

        PlanResponseDto plan = new PlanResponseDto(
            subject.Id, "Тест", 8, 7, true, "Достигнуто",
            new List<OptimizationItemDto>(),
            new List<GradePlanDto>(),
            false, null, null);

        mock.Setup(m => m.UpdateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync((plan, ApiError.None));

        mock.Setup(m => m.CalculatePlanAsync(subject.Id))
            .ReturnsAsync(plan);

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        vm.CalculatePlanCommand.Execute(null);
        await WaitForLoadingCycle(vm);

        Assert.False(vm.IsLoading);
        Assert.Contains("достигнута", vm.StatusMessage.ToLowerInvariant());
    }

    [AvaloniaFact]
    public void ExecuteCalculatePlan_WhenNoSubject_DoesNothing()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> _);
        vm.SelectedSubject = null;

        vm.CalculatePlanCommand.Execute(null);

        Assert.Null(vm.CurrentPlan);
        Assert.False(vm.IsLoading);
    }

    [AvaloniaFact]
    public void ExecuteCalculatePlan_WhenFormulaInvalid_DoesNothing()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> _);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        subject.Formula.Add(new GradeComponent { Weight = 30, Complexity = 5, CurrentGrade = 5 });
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        Assert.False(vm.CanCalculatePlan);

        vm.CalculatePlanCommand.Execute(null);

        Assert.Null(vm.CurrentPlan);
    }

    [AvaloniaFact]
    public async Task DeleteSubject_WhenWasOnServer_CallsDelete()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        MarkSynced(vm, subject.Id);

        mock.Setup(m => m.DeleteSubjectAsync(subject.Id))
            .ReturnsAsync(true);

        vm.DeleteSubjectCommand.Execute(null);

        await Task.Delay(100);

        mock.Verify(m => m.DeleteSubjectAsync(subject.Id), Times.Once);
        Assert.Empty(vm.Subjects);
        Assert.Null(vm.SelectedSubject);
    }

    [AvaloniaFact]
    public async Task DeleteSubject_WhenWasOnServerAndDeleteFails_SetsErrorStatus()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        await Task.Delay(500);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        MarkSynced(vm, subject.Id);

        mock.Setup(m => m.DeleteSubjectAsync(subject.Id))
            .ReturnsAsync(false);

        vm.DeleteSubjectCommand.Execute(null);

        await Task.Delay(300);

        Assert.Contains("не удалось", vm.ServerStatus.ToLowerInvariant());
    }

    [AvaloniaFact]
    public async Task DeleteSubject_WhenWasNotOnServer_DoesNotCallDelete()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;

        vm.DeleteSubjectCommand.Execute(null);

        await Task.Delay(50);

        mock.Verify(m => m.DeleteSubjectAsync(It.IsAny<Guid>()), Times.Never);
        Assert.Empty(vm.Subjects);
    }

    [AvaloniaFact]
    public void DeleteSubject_WhenNoSubjectSelected_DoesNothing()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        vm.SelectedSubject = null;

        MethodInfo? method = typeof(MainViewModel).GetMethod(
            "ExecuteDeleteSubject",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        method!.Invoke(vm, null);

        mock.Verify(m => m.DeleteSubjectAsync(It.IsAny<Guid>()), Times.Never);
    }

    [AvaloniaFact]
    public async Task LoadSubjectsAsync_WhenServerReturnsSubjects_PopulatesCollection()
    {
        Mock<IApiService> mock = new Mock<IApiService>();
        Subject s1 = new Subject { Name = "Математика", TargetGrade = 7 };
        Subject s2 = new Subject { Name = "Физика", TargetGrade = 8 };
        ObservableCollection<Subject> loaded = new ObservableCollection<Subject> { s1, s2 };

        mock.Setup(m => m.LoadSubjectsAsync())
            .ReturnsAsync((loaded, true));

        MainViewModel vm = new MainViewModel();
        await Task.Delay(500);

        InjectApiService(vm, mock.Object);

        MethodInfo? method = typeof(MainViewModel).GetMethod(
            "LoadSubjectsAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(vm, null);

        await Task.Delay(300);

        Assert.Equal(2, vm.Subjects.Count);
        Assert.Contains("Загружено", vm.StatusMessage);
    }

    [AvaloniaFact]
    public async Task LoadSubjectsAsync_WhenServerUnavailable_SetsServerStatus()
    {
        Mock<IApiService> mock = new Mock<IApiService>();
        mock.Setup(m => m.LoadSubjectsAsync())
            .ReturnsAsync((new ObservableCollection<Subject>(), false));

        MainViewModel vm = new MainViewModel();
        await Task.Delay(500);

        InjectApiService(vm, mock.Object);

        MethodInfo? method = typeof(MainViewModel).GetMethod(
            "LoadSubjectsAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(vm, null);

        await Task.Delay(300);

        Assert.Contains("сервер", vm.ServerStatus.ToLowerInvariant());
    }

    [AvaloniaFact]
    public async Task LoadSubjectsAsync_WhenEmpty_SetsAddFirstMessage()
    {
        Mock<IApiService> mock = new Mock<IApiService>();
        mock.Setup(m => m.LoadSubjectsAsync())
            .ReturnsAsync((new ObservableCollection<Subject>(), true));

        MainViewModel vm = new MainViewModel();
        await Task.Delay(500);

        InjectApiService(vm, mock.Object);

        MethodInfo? method = typeof(MainViewModel).GetMethod(
            "LoadSubjectsAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(vm, null);

        await Task.Delay(300);

        Assert.Contains("первый", vm.StatusMessage.ToLowerInvariant());
    }

    [AvaloniaFact]
    public async Task SyncSubjectAsync_WhenCalledThroughReflection_DoesNotThrow()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        Subject subject = BuildValidSubject();
        MarkSynced(vm, subject.Id);

        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        mock.Setup(m => m.UpdateSubjectAsync(It.IsAny<Subject>()))
            .ReturnsAsync(((PlanResponseDto?)null, ApiError.None));

        MethodInfo? method = typeof(MainViewModel).GetMethod(
            "SyncSubjectAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        Task task = (Task)method!.Invoke(vm, null)!;
        await task;

        mock.Verify(m => m.UpdateSubjectAsync(subject), Times.Once);
    }

    [AvaloniaFact]
    public async Task SyncSubjectAsync_WhenNoSubject_DoesNothing()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);
        vm.SelectedSubject = null;

        MethodInfo? method = typeof(MainViewModel).GetMethod(
            "SyncSubjectAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Task task = (Task)method!.Invoke(vm, null)!;
        await task;

        mock.Verify(m => m.UpdateSubjectAsync(It.IsAny<Subject>()), Times.Never);
    }

    [AvaloniaFact]
    public async Task SyncSubjectAsync_WhenFormulaInvalid_DoesNothing()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);

        Subject subject = new Subject { Name = "Тест", TargetGrade = 6 };
        subject.Formula.Add(new GradeComponent { Weight = 30, Complexity = 5, CurrentGrade = 5 });
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        MarkSynced(vm, subject.Id);

        MethodInfo? method = typeof(MainViewModel).GetMethod(
            "SyncSubjectAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Task task = (Task)method!.Invoke(vm, null)!;
        await task;

        mock.Verify(m => m.UpdateSubjectAsync(It.IsAny<Subject>()), Times.Never);
    }

    [AvaloniaFact]
    public async Task SyncSubjectAsync_WhenSubjectNotSynced_DoesNothing()
    {
        MainViewModel vm = CreateVmWithMock(out Mock<IApiService> mock);

        Subject subject = BuildValidSubject();
        vm.Subjects.Add(subject);
        vm.SelectedSubject = subject;
        vm.RefreshFormulaStats();

        MethodInfo? method = typeof(MainViewModel).GetMethod(
            "SyncSubjectAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Task task = (Task)method!.Invoke(vm, null)!;
        await task;

        mock.Verify(m => m.UpdateSubjectAsync(It.IsAny<Subject>()), Times.Never);
    }
}
