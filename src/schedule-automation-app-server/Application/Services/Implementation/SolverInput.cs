namespace schedule_automation_app_server.Application.Services.Implementation;

public class SolverInput
{
    public double[] CurrentGrades { get; }
    public double[] Weights { get; }
    public int[] Complexities { get; }
    public double TargetWeightedSum { get; }

    public SolverInput(double[] currentGrades, double[] weights, int[] complexities, double targetWeightedSum)
    {
        CurrentGrades = currentGrades;
        Weights = weights;
        Complexities = complexities;
        TargetWeightedSum = targetWeightedSum;
    }
}