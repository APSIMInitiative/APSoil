using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;
using API.Models;
using APSIM.Shared.Utilities;
using Graph;

namespace API.Services;

public static class Extensions
{
    /// <summary>Convert an object to an XML string.</summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>An XML string.</returns>
    public static string ToXML<T>(this T obj)
    {
        // Serialize the object synchronously then rewind the stream
        XmlSerializer Serializer = new(obj.GetType());
        using var textWriter = new StringWriter();
        Serializer.Serialize(textWriter, obj);
        return textWriter.ToString();
    }

    /// <summary>Convert an HTTP request to an XML string.</summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>An XML string.</returns>
    public static string ToXML(this HttpRequest request)
    {
        using StreamReader body = new StreamReader(request.Body);
        var task = body.ReadToEndAsync();
        task.Wait();
        return task.Result;
    }

    /// <summary>
    /// Convert a collection of text strings to an IResult
    /// </summary>
    /// <param name="text">The text string.</param>
    /// <returns>A custom IResult.</returns>
    public static IResult ToImageResult(this byte[] bytes)
    {
        return new CustomResult<byte[]>(bytes, "image/png");
    }

    /// <summary>
    /// Convert an array of soils to an XML string.
    /// </summary>
    /// <param name="soils">The array of soils.</param>
    /// <returns>An XML string.</returns>
    public static Folder ToFolder(this API.Models.Soil[] soils)
    {
        return new Models.Folder()
        {
            Name = "Soils",
            Soils = soils.ToList()
        };
    }

    /// <summary>
    /// Convert an XML string to an array of soils.
    /// </summary>
    /// <param name="xml">The XML string.</param>
    /// <returns>An array of soils.</returns>
    public static Models.Soil[] ToSoils(this string xml)
    {
        // Old APSoil format uses lowercase folder tag for the root folder.
        xml = xml.Replace("folder", "Folder").Replace("NaN", "0");

        var serializer = new XmlSerializer(typeof(Models.Folder));
        using var reader = new StringReader(xml);
        var folder = (Models.Folder)serializer.Deserialize(reader);
        return GetSoilsRecursively(folder).ToArray();
    }

    /// <summary>Convert a soil to a basic info.</summary>
    /// <param name="soil">The soil.</param>
    /// <returns>The basic soil info instance.</returns>
    public static Models.BasicInfo ToBasicInfo(this Models.Soil soil)
    {
        return new Models.BasicInfo
        {
            Name = soil.FullName,
            Latitude = soil.Latitude,
            Longitude = soil.Longitude,
        };
    }

    /// <summary>Convert a soil to a soil info.</summary>
    /// <param name="soil">The soil.</param>
    /// <returns>The soil info.</returns>
    public static Models.SoilInfo ToInfo(this Models.Soil soil)
    {
        return new Models.SoilInfo
        {
            Name = soil.FullName == null ? soil.Name : soil.FullName,
            Description = soil.DataSource,
            SoilType = soil.SoilType,
            Latitude = soil.Latitude,
            Longitude = soil.Longitude,
            Distance = 0,
            ASCOrder = soil.ASCOrder,
            ASCSubOrder = soil.ASCSubOrder,
            Site = soil.Site,
            Region = soil.Region,
            NearestTown = soil.NearestTown,
            Thickness = soil.Water?.Thickness.ToArray(),
            Texture = soil.Analysis?.Texture.ToArray(),
            DUL = soil.Water?.DUL.ToArray(),
            LL15 = soil.Water?.LL15.ToArray(),
            EC = soil.Analysis?.EC.ToArray(),
            CL = soil.Analysis?.CL.ToArray(),
            ESP = soil.Analysis?.ESP.ToArray(),
            PH = soil.Analysis?.PH.ToArray(),
            Crops = soil.Water?.SoilCrops.Select(c => new Models.SoilCropInfo
            {
                Name = c.Name,
                LL = c.LL.ToArray(),
                KL = c.KL.ToArray(),
                XF = c.XF.ToArray()
            }).ToList()
        };
    }

    public static byte[] ToGraphPng(this Models.Soil soil, double[] thickness = null, double[] sw = null, bool swIsGrav = false)
    {
        double[] midPoints = SoilUtilities.ToMidPoints(soil.Water.Thickness.ToArray());

        GraphModel graph = new()
        {
            Title = soil.Name,
            LegendPosition = GraphModel.LegendPositionEnum.BottomLeft,
            Axes =
            [
                new()
                {
                    Title = "Depth (mm)",
                    Minimum = 0,
                    Position = AxisModel.PositionEnum.Left,
                    Maximum = (Math.Truncate(midPoints.Last() / 200) + 1) * 200,  // Scale up to the nearest 200
                    IsVisible = true,
                    Inverted = true
                },
                new()
                {
                    Title = "Volumetric Water Content (mm/mm)",
                    Position = AxisModel.PositionEnum.Top,
                    IsVisible = true
                }
            ]
        };
        double[] ll;
        double[] xf;
        double[] pawc;
        string llName;
        if (soil.Water.SoilCrops == null || soil.Water.SoilCrops.Count == 0)
        {
            ll = soil.Water.LL15.ToArray();
            llName = "LL15";
            xf = null;
        }
        else
        {
            // Look for wheat soil crop. If not found then use the first soil crop.
            if (soil.Water.SoilCrops.Any(c => c.Name == "wheat"))
            {
                ll = soil.Water.SoilCrops.First(c => c.Name == "wheat").LL.ToArray();
                llName = "wheat";
                xf = soil.Water.SoilCrops.First(c => c.Name == "wheat").XF.ToArray();
            }
            else
            {
                ll = soil.Water.SoilCrops.First().LL.ToArray();
                llName = soil.Water.SoilCrops.First().Name;
                xf = soil.Water.SoilCrops.First().XF.ToArray();
            }
        }
        pawc = SoilUtilities.CalcPAWC(soil.Water.Thickness.ToArray(), ll, soil.Water.DUL.ToArray(), xf);
        pawc = MathUtilities.Multiply(pawc, soil.Water.Thickness.ToArray()); // Convert to mm

        graph.Series =
        [
            new SeriesModel()
            {
                Title = $"{llName} PAWC: {pawc.Sum():F0} mm",
                SeriesType = SeriesModel.SeriesTypeEnum.Area,
                Points = soil.Water.LL15.Zip(midPoints)
                                        .Select(zip => new DataPoint { X = zip.First, Y = zip.Second }),
                Points2 = soil.Water.DUL.Zip(midPoints)
                                        .Select(zip => new DataPoint { X = zip.First, Y = zip.Second }),
                ShowInLegend = true,
                LineType = SeriesModel.LineTypeEnum.Solid,
                Colour = Color.LightBlue,
            },
            new SeriesModel()
            {
                Title = "Airdry",
                Points = soil.Water.AirDry.Zip(midPoints)
                                        .Select(zip => new DataPoint { X = zip.First, Y = zip.Second }),
                ShowInLegend = true,
                LineType = SeriesModel.LineTypeEnum.Dot,
                Colour = Color.Red,
            },
            new()
            {
                Title = "LL15",
                Points = soil.Water.LL15.Zip(midPoints)
                                        .Select(zip => new DataPoint { X = zip.First, Y = zip.Second }),
                ShowInLegend = true,
                LineType = SeriesModel.LineTypeEnum.Solid,
                Colour = Color.Red,
            },
            new()
            {
                Title = "DUL",
                Points = soil.Water.DUL.Zip(midPoints)
                                        .Select(zip => new DataPoint { X = zip.First, Y = zip.Second }),
                ShowInLegend = true,
                LineType = SeriesModel.LineTypeEnum.Solid,
                Colour = Color.Blue,
            },
            new SeriesModel()
            {
                Title = "SAT",
                Points = soil.Water.SAT.Zip(midPoints)
                                        .Select(zip => new DataPoint { X = zip.First, Y = zip.Second }),
                ShowInLegend = true,
                LineType = SeriesModel.LineTypeEnum.Dot,
                Colour = Color.Blue,
            }
        ];

        if (thickness != null && sw != null)
        {
            double[] swMidPoints = SoilUtilities.ToMidPoints(thickness.ToArray());
            graph.Series.Add(new SeriesModel()
            {
                Title = "SW",
                Points = sw.Zip(swMidPoints)
                           .Select(zip => new DataPoint { X = zip.First, Y = zip.Second }),
                ShowInLegend = true,
                LineType = SeriesModel.LineTypeEnum.Solid,
                Colour = Color.Green,
            });
        }
        return GraphRenderToPNG.Render(graph);
    }

    public static IReadOnlyList<double> ConvertGravimetricToVolumetric(this IReadOnlyList<double> gravimetricWater, IReadOnlyList<double> bulkDensity)
    {
        if (gravimetricWater.Count != bulkDensity.Count)
            throw new ArgumentException("Soil water and bulk density arrays must be the same length.");

        return MathUtilities.Multiply(gravimetricWater, bulkDensity);
    }

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

    /// <summary>Join a collection of strings together with a delimiter between each.</summary>
    /// <param name="strings">The collection of strings.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns></returns>
    /// <returns>The new collection that was created.</returns>
    private static string Join<T>(this IEnumerable<T> strings, string delimiter)
    {
        var writer = new StringBuilder();
        foreach (var st in strings)
        {
            if (writer.Length > 0)
                writer.Append(delimiter);
            writer.Append(st);
        }
        return writer.ToString();
    }

    /// <summary>Get all folders and subfolders recursively.</summary>
    /// <param name="folder">The root folder.</param>
    /// <returns>All folders and subfolders.</returns>
    private static IEnumerable<Models.Soil> GetSoilsRecursively(Models.Folder folder, string parentFolderPath = null)
    {
        string path = $"{parentFolderPath}{folder.Name}/";
        foreach (var subFolder in folder.Folders)
           foreach (var soil in GetSoilsRecursively(subFolder, path))
                yield return soil;

        foreach (var soil in folder.Soils)
        {
            soil.FullName = $"{path}{soil.Name}";
            yield return soil;
        }
    }

}