// https://code-maze.com/aspnetcore-query-string-parameters-minimal-apis/


public class Values
{
    public string[] Strings { get; private set; } = null;
    public double[] Doubles => Strings.Select(s => double.Parse(s)).ToArray();
    public static bool TryParse(string value, IFormatProvider provider, out Values arr)
    {
        arr = new Values() { Strings = value.Split(',') };
        return true;
    }
}
