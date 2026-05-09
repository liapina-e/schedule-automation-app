using Microsoft.Extensions.Logging.Abstractions;
using schedule_automation_app_server.Application.DTOs;
using schedule_automation_app_server.Application.Mappers;
using schedule_automation_app_server.Application.Services.Implementation;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Tests;

public class SolverInputTests
{

    [Fact]
    public void SolverInput_StoresAllProperties()
    {
        double[] current = { 1, 2 };
        double[] weights = { 0.5, 0.5 };
        int[] complexities = { 3, 7 };
        double targetSum = 6.0;
        bool[] isBlocking = { false, true };
        double[] minimumGrades = { 0, 4.0 };

        SolverInput input = new SolverInput(current, weights, complexities, targetSum, isBlocking, minimumGrades);

        Assert.Equal(current, input.CurrentGrades);
        Assert.Equal(weights, input.Weights);
        Assert.Equal(complexities, input.Complexities);
        Assert.Equal(targetSum, input.TargetWeightedSum);
        Assert.Equal(isBlocking, input.IsBlocking);
        Assert.Equal(minimumGrades, input.MinimumGrades);
    }
    
}