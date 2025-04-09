using System.Drawing;

namespace Graph;

public class GraphModel
{
    public string Title { get; set; }
    public List<AxisModel> Axes { get; set; }
    public List<SeriesModel> Series { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool ShowLegend { get; set; }
    public enum LegendPositionEnum { TopLeft, BottomLeft, TopRight, BottomRight }
    public LegendPositionEnum LegendPosition { get; set; }
}

public class SeriesModel
{
    public string Title { get; set; }
    public IEnumerable<DataPoint> Points { get; set; }
    public IEnumerable<DataPoint> Points2 { get; set; }
    public bool ShowInLegend { get; set; }
    public enum LineTypeEnum { Solid, Dash, Dot, None }
    public LineTypeEnum LineType { get; set; }
    public Color Colour { get; set; }
    public Color BackgroundColour { get; set; }
    public enum MarkerTypeEnum { None, Circle, Square, Diamond, Triangle, Cross }
    public MarkerTypeEnum MarkerType { get; set; }
    public enum SeriesTypeEnum { XY, Area }
    public SeriesTypeEnum SeriesType { get; set; }
}

public class DataPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class AxisModel
{
    public string Title { get; set; }
    public enum PositionEnum { Left, Right, Top, Bottom }
    public PositionEnum Position { get; set; }
    public double Minimum { get; set; } = double.NaN;
    public double Maximum { get; set; } = double.NaN;
    public double Interval { get; set; } = double.NaN;
    public bool IsVisible { get; set; }
    public bool Inverted { get; set; }
}
