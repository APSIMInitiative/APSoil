namespace APSIM.Shared.Utilities;

public static class MathExtensions
{
    /// <summary>
    /// Perform a stepwise multiply of the values in value 1 with the values in value2.
    /// Returns an array of the same size as value 1 and value 2
    /// </summary>
    public static double[] Multiply(this IReadOnlyList<double> value1, IReadOnlyList<double> value2)
    {
        double[] results = new double[value1.Count];
        if (value1.Count == value2.Count)
        {
            results = new double[value1.Count];
            for (int iIndex = 0; iIndex < value1.Count; iIndex++)
                results[iIndex] = value1[iIndex] * value2[iIndex];
        }
        return results;
    }

    /// <summary>
    /// Perform a stepwise Divide of the values in value 1 with the values in value2.
    /// Returns an array of the same size as value 1 and value 2
    /// </summary>
    public static double[] Divide(this IReadOnlyList<double> value1, IReadOnlyList<double> value2, double errVal=0.0)
    {
        double[] results = null;
        if (value1.Count == value2.Count)
        {
            results = new double[value1.Count];
            for (int iIndex = 0; iIndex < value1.Count; iIndex++)
                results[iIndex] = MathUtilities.Divide(value1[iIndex], value2[iIndex], errVal);
        }
            return results;
    }

    /// <summary>
    /// Constrain the values in value1 to be greater than the values in value2.
    /// </summary>
    public static IReadOnlyList<double> LowerConstraint(this IReadOnlyList<double> values, IReadOnlyList<double> lower, int startIndex = 0)
    {
        if (values.Count != lower.Count)
            throw new Exception("The two arrays must be the same length.");
        var results = values.ToArray();
        for (int iIndex = startIndex; iIndex < values.Count; iIndex++)
            results[iIndex] = Math.Max(values[iIndex], lower[iIndex]);
        return results;
    }
}