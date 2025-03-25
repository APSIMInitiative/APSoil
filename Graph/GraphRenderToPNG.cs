using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.ImageSharp;
using OxyPlot.Legends;
using OxyPlot.Series;
using LegendPlacement = OxyPlot.Legends.LegendPlacement;
using OxyLegendOrientation = OxyPlot.Legends.LegendOrientation;
using OxyLegendPosition = OxyPlot.Legends.LegendPosition;

namespace Graph;

public class GraphRenderToPNG
{
    public static byte[] Render(GraphModel graphModel)
    {
        PlotModel plotModel = ToPlotModel(graphModel);

        using var stream = new MemoryStream();
        var pngExporter = new PngExporter(width: 600, height: 800);
        pngExporter.Export(plotModel, stream);
        stream.Seek(0, SeekOrigin.Begin);
        return stream.ToArray();
    }

    /// <summary>
    /// Converts the Graph model to a PlotModel
    /// </summary>
    /// <param name="graphModel">The Graph model</param>
    /// <returns>The PlotModel</returns>
    private static PlotModel ToPlotModel(GraphModel graphModel)
    {
        PlotModel plotModel = new()
        {
            Title = graphModel.Title,
            Background = OxyColors.White,
            DefaultFontSize = 18,
            DefaultFont = "Arial",
            PlotAreaBorderThickness = new(0),
        };

        foreach (var axis in graphModel.Axes.Where(a => a.IsVisible))
        {
            var position = axis.Position switch
            {
                AxisModel.PositionEnum.Left => AxisPosition.Left,
                AxisModel.PositionEnum.Right => AxisPosition.Right,
                AxisModel.PositionEnum.Top => AxisPosition.Top,
                AxisModel.PositionEnum.Bottom => AxisPosition.Bottom,
                _ => AxisPosition.Left
            };

            var axisModel = new LinearAxis
            {
                Position = position,
                Title = axis.Title,
                Minimum = axis.Minimum,
                Maximum = axis.Maximum,
                IsAxisVisible = axis.IsVisible,
                StartPosition = axis.Inverted ? 1 : 0,
                EndPosition = axis.Inverted ? 0 : 1,
                AxislineStyle = LineStyle.Solid,
            };

            plotModel.Axes.Add(axisModel);
        }

        foreach (var series in graphModel.Series)
        {
            LineSeries lineSeries;
            if (series.SeriesType == SeriesModel.SeriesTypeEnum.XY)
                lineSeries = new LineSeries();
            else
            {
                var areaSeries = new AreaSeries();
                foreach (var point in series.Points2)
                    areaSeries.Points2.Add(new OxyPlot.DataPoint(point.X, point.Y));
                lineSeries = areaSeries;
            }

            lineSeries.Color = OxyColor.FromArgb(series.Colour.A, series.Colour.R, series.Colour.G, series.Colour.B);
            lineSeries.Background = OxyColor.FromArgb(series.BackgroundColour.A, series.BackgroundColour.R, series.BackgroundColour.G, series.BackgroundColour.B);
            lineSeries.LineStyle = series.LineType switch
            {
                SeriesModel.LineTypeEnum.Solid => LineStyle.Solid,
                SeriesModel.LineTypeEnum.Dash => LineStyle.Dash,
                SeriesModel.LineTypeEnum.Dot => LineStyle.Dot,
                SeriesModel.LineTypeEnum.None => LineStyle.None,
                _ => LineStyle.Solid
            };
            lineSeries.MarkerType = series.MarkerType switch
            {
                SeriesModel.MarkerTypeEnum.None => MarkerType.None,
                SeriesModel.MarkerTypeEnum.Circle => MarkerType.Circle,
                SeriesModel.MarkerTypeEnum.Square => MarkerType.Square,
                SeriesModel.MarkerTypeEnum.Diamond => MarkerType.Diamond,
                SeriesModel.MarkerTypeEnum.Triangle => MarkerType.Triangle,
                SeriesModel.MarkerTypeEnum.Cross => MarkerType.Cross,
                _ => MarkerType.Circle
            };
            lineSeries.MarkerSize = 6;
            lineSeries.Title = series.Title;

            foreach (var point in series.Points)
                lineSeries.Points.Add(new OxyPlot.DataPoint(point.X, point.Y));
            plotModel.Series.Add(lineSeries);
        }

        // Add legend if required
        if (graphModel.Series.Any(s => s.ShowInLegend))
        {
            var legend = new Legend
            {
                LegendPosition = graphModel.LegendPosition switch
                {
                    GraphModel.LegendPositionEnum.TopLeft => OxyLegendPosition.TopLeft,
                    GraphModel.LegendPositionEnum.BottomLeft => OxyLegendPosition.BottomLeft,
                    GraphModel.LegendPositionEnum.TopRight => OxyLegendPosition.TopRight,
                    GraphModel.LegendPositionEnum.BottomRight => OxyLegendPosition.BottomRight,
                    _ => OxyLegendPosition.TopLeft
                },
                LegendOrientation = OxyLegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Inside,
                LegendSymbolLength = 30,
            };
            plotModel.Legends.Add(legend);
        }
        return plotModel;
    }
}
