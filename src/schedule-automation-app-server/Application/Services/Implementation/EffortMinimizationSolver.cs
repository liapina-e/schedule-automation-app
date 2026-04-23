namespace schedule_automation_app_server.Application.Services.Implementation;

public class EffortMinimizationSolver
{
    public double[] Solve(
        double[] currentGrades,
        double[] weights,
        int[] complexities,
        double targetWeightedSum)
    {
        int n = currentGrades.Length;
        double[] result = new double[n];
        Array.Copy(currentGrades, result, n);

        double alreadyCovered = result.Select((g, i) => g * weights[i]).Sum();
        double remaining = targetWeightedSum - alreadyCovered;

        if (remaining <= 1e-9)
        {
            return result;
        }

        int[] order = Enumerable.Range(0, n)
            .Where(i => weights[i] > 1e-9)
            .OrderBy(i => complexities[i] / weights[i])
            .ToArray();

        foreach (int i in order)
        {
            if (remaining <= 1e-9)
            {
                break;
            }

            double canGain = (10.0 - result[i]) * weights[i];

            if (canGain <= 1e-9)
            {
                continue;
            }

            if (canGain <= remaining)
            {
                result[i] = 10.0;
                remaining -= canGain;
            }
            else
            {
                result[i] += remaining / weights[i];
                remaining = 0;
            }
        }

        return result;
    }
}
