using System.Drawing;
using System.Text;
using System.Xml.Serialization;
using API.Data;
using API.Models;
using APSIM.Shared.Utilities;
using Graph;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public static class Soil
{
    /// <summary>
    /// Add soils to the database or update them if they already exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="soil">The soil to add.</param>
    /// <returns>Created<API.Models.Soil> if success, otherwise Results.Problem</returns>
    public static void Add(SoilDbContext context, Models.Soil[] soils)
    {
        foreach (var soil in soils)
        {
            // If soil doesn't have a full name then make the full name the same as 'Name'
            if (string.IsNullOrEmpty(soil.FullName))
                soil.FullName = soil.Name;

            // Ensure all crop names are lowercase. This makes the querys later able to be done via SQL, rather than in-memory.
            foreach (var crop in soil.Water.SoilCrops)
                crop.Name = crop.Name.ToLower();

            if (context.Soils.Any(s => s.FullName == soil.FullName))
                context.Soils.Update(soil);
            else
                context.Soils.Add(soil);
        }
        context.SaveChanges();
    }

    /// <summary>Search for soils and return matching full names.</summary>
    /// <param name="context">The database context.</param>
    /// <param name="name">The name of the soil.</param>
    /// <param name="folder">The folder containing the soil.</param>
    /// <param name="soilType">The type of soil.</param>
    /// <param name="latitude">The latitude of the point.</param>
    /// <param name="longitude">The longitude of the point.</param>
    /// <param name="fullName">The full name of the soil.</param>
    /// <param name="cropName">The name of the crop to use when cll or pawc is provided.</param>
    /// <param name="thickness">The thickness of the cll or PAWC values.</param>
    /// <param name="cll">The crop lower limit (volumetric).</param>
    /// <param name="pawc">The plant available water capacity by layer (mm).</param>
    /// <returns>An array of full soil names.</returns>
    public static string[] Search(SoilDbContext context, string name = null, string folder = null, string soilType = null, string country = null,
                                  double latitude = double.NaN, double longitude = double.NaN, double radius = double.NaN,
                                  string fullName = null,
                                  string cropName = null, double[] thickness = null, double[] cll = null, double[] pawc = null,
                                  int numToReturn = 0)
    {
        IQueryable<Models.Soil> soils = context.Soils;
        if (name != null)
            soils = soils.Where(s => s.Name.Contains(name));
        if (fullName != null)
            soils = soils.Where(s => s.FullName == fullName);
        if (folder != null)
            soils = soils.Where(s => s.FullName.Contains(folder));
        if (soilType != null)
            soils = soils.Where(s => s.SoilType.Contains(soilType));
        if (country != null)
            soils = soils.Where(s => s.Country == country);
        if (cropName != null)
            soils = soils.Where(s => s.Water.SoilCrops.Any(sc => sc.Name == cropName.ToLower()));

        // The above where clauses can be converted to SQL by EntityFramework.
        // The lat/long, cll and pawc orderby clauses below cannot be converted to SQL so must be done in memory by LINQ
        if (double.IsNaN(latitude) && double.IsNaN(longitude) && cll == null && pawc == null)
        {
            // return only the required number of soils.
            if (numToReturn > 0)
                soils = soils.Take(numToReturn);

            // return full soil, forcing eager loading of related entities.
            return soils.Select(s => s.FullName).ToArray();
        }
        else
        {
            // If there are performance issues with this code, some fields could be added to soil
            // for pawc and cll for predefined layers (e.g. 0-300, 300-600, 600-900 etc) to allow
            // the query to be done in SQL (rather than needing a 'Difference' function).
            IEnumerable<Models.Soil> soilsInMemory = soils.Include(s => s.Water)
                                                          .Include(s => s.Water.SoilCrops)
                                                          .ToList();

            if (!double.IsNaN(latitude) && !double.IsNaN(longitude))
            {
                if (!double.IsNaN(radius))
                    soilsInMemory = soilsInMemory.Where(s => Distance(s, latitude, longitude) <= radius);
                soilsInMemory = soilsInMemory.OrderBy(s => Distance(s, latitude, longitude));
            }

            if (cll != null)
                soilsInMemory = soilsInMemory.OrderBy(s => Difference(cll, s.Crop(cropName)
                                                                            .LL
                                                                            .MappedTo(s.Water.Thickness, thickness))
                                                           .Sum());

            if (pawc != null)
            {
                soilsInMemory = soilsInMemory.OrderBy(s => Difference(pawc, s.Crop(cropName)
                                                                             .PAWC(s.Water.DUL)
                                                                             .ToMM(s.Water.Thickness)
                                                                             .MappedTo(s.Water.Thickness, thickness)
                                                                     )
                                                           .Sum());
            }
            // return only the required number of soils.
            if (numToReturn > 0)
                soilsInMemory = soilsInMemory.Take(numToReturn);
            return soilsInMemory.Select(s => s.FullName).ToArray();
        }
    }

    /// <summary>Convert a soil to a soil info.</summary>
    /// <param name="soil">The soil.</param>
    /// <returns>The soil info.</returns>
    public static Models.SoilInfo ToInfo(this Models.Soil soil)
    {
        return new Models.SoilInfo
        {
            Name = soil.Name,
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
            Thickness = soil.Water.Thickness.ToArray(),
            Texture = soil.Analysis.Texture.ToArray(),
            DUL = soil.Water.DUL.ToArray(),
            LL15 = soil.Water.LL15.ToArray(),
            EC = soil.Analysis.EC.ToArray(),
            CL = soil.Analysis.CL.ToArray(),
            ESP = soil.Analysis.ESP.ToArray(),
            PH = soil.Analysis.PH.ToArray(),
            Crops = soil.Water.SoilCrops.Select(c => new Models.SoilCropInfo
            {
                Name = c.Name,
                LL = c.LL.ToArray(),
                KL = c.KL.ToArray(),
                XF = c.XF.ToArray()
            }).ToList()
        };
    }

    /// <summary>Get the soils for the specified full paths.</summary>
    /// <param name="context">The database context.</param>
    /// <returns>An array of soils.</returns>
    public static Models.Soil[] Get(SoilDbContext context, string[] fullNames)
    {
        List<Models.Soil> soilsToReturn = new();
        IQueryable<Models.Soil> soils = context.Soils;
        foreach (var fullName in fullNames)
            soilsToReturn.AddRange(soils.Where(s => s.FullName == fullName)
                                        .Include(s => s.Water)
                                        .Include(s => s.Water.SoilCrops)
                                        .Include( s=> s.Analysis)
                                        .Include( s=> s.SoilWater)
                                        .Include( s=> s.SoilOrganicMatter)
                                        .Include( s=> s.Water.SoilCrops));
        return soilsToReturn.ToArray();
    }

    public static byte[] ToGraphPng(this Models.Soil soil)
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
            },


        ];
        return GraphRenderToPNG.Render(graph);
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

    /// <summary>
    /// Convert an array of soils to an XML string.
    /// </summary>
    /// <param name="soils">The array of soils.</param>
    /// <returns>An XML string.</returns>
    public static Folder ToFolder(this Models.Soil[] soils)
    {
        return new Models.Folder()
        {
            Name = "Soils",
            Soils = soils.ToList()
        };
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
    /// Convert an array of soils to an XML IResult
    /// </summary>
    /// <param name="soils">The array of soils.</param>
    /// <returns>A custom IResult.</returns>
    public static IResult ToXMLResult<T>(this T soils)
    {
        return new CustomResult<string>(soils.ToXML(), "application/xml");
    }

    /// <summary>
    /// Convert a collection of text strings to an IResult
    /// </summary>
    /// <param name="text">The text string.</param>
    /// <returns>A custom IResult.</returns>
    public static IResult ToTextResult(this string[] text)
    {
        return new CustomResult<string>(text.Join("\n"), "text/plain");
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

    /// <summary>Convert an object to an XML string.</summary>
    /// <param name="folder">The object to convert.</param>
    /// <returns>An XML string.</returns>
    private static string ToXML<T>(this T folder)
    {
        // Serialize the object synchronously then rewind the stream
        XmlSerializer Serializer = new(typeof(T));
        using var textWriter = new StringWriter();
        Serializer.Serialize(textWriter, folder);
        return textWriter.ToString();
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

    /// <summary>Get the distance between a soil and a point.</summary>
    /// <param name="soil">The soil.</param>
    /// <param name="latitude">The latitude of the point.</param>
    /// <param name="longitude">The longitude of the point.</param>
    /// <returns>The distance between the soil and the point (km).</returns>
    private static double Distance(Models.Soil soil, double latitude, double longitude)
    {
        double theta = soil.Longitude - longitude;
        double dist = Math.Sin(deg2rad(soil.Latitude)) * Math.Sin(deg2rad(latitude)) + Math.Cos(deg2rad(soil.Latitude)) * Math.Cos(deg2rad(latitude)) * Math.Cos(deg2rad(theta));
        dist = Math.Acos(dist);
        dist = rad2deg(dist);
        dist = dist * 60 * 1.1515;
        return dist * 1.609344; // To kilometers
    }

    /// <summary>
    /// This function converts decimal degrees to radians
    /// </summary>
    /// <param name="deg">The decimal degrees to convert.</param>
    /// <returns>The radians.</returns>
    private static double deg2rad(double deg)
    {
        return deg * Math.PI / 180.0;
    }

    /// <summary>
    /// This function converts radians to decimal degrees
    /// </summary>
    /// <param name="rad">The radians to convert.</param>
    /// <returns>The decimal degrees.</returns>
    private static double rad2deg(double rad)
    {
        return rad / Math.PI * 180.0;
    }

    /// <summary>Get the crop lower limit (volumetric) for a soil.</summary>
    /// <param name="cropName">The crop name.</param>
    /// <returns>The crop lower limit.</returns>
    private static SoilCrop Crop(this Models.Soil soil, string cropName)
    {
        if (soil.Water == null || soil.Water.SoilCrops == null)
            throw new Exception("Soil has no soilcrop data.");
        var crop = soil.Water.SoilCrops.FirstOrDefault(c => c.Name == cropName);
        if (crop == null)
            throw new Exception($"Soil has no soilcrop data for {cropName}.");
        return crop;
    }

    /// <summary>Get the crop lower limit (volumetric) for a soil.</summary>
    /// <param name="cropName">The crop name.</param>
    /// <returns>The crop lower limit.</returns>
    private static double[] CLL(this Models.Soil soil, string cropName)
    {
        if (soil.Water == null || soil.Water.SoilCrops == null)
            throw new Exception("Soil has no soilcrop data.");
        var crop = soil.Water.SoilCrops.FirstOrDefault(c => c.Name == cropName);
        if (crop == null)
            throw new Exception($"Soil has no soilcrop data for {cropName}.");
        return crop.LL.ToArray();
    }

    private static double[] PAWC(this Models.SoilCrop crop, IList<double> dul)
    {
        if (crop.LL == null)
            throw new Exception($"SoilCrop {crop.Name} has no LL data.");
        return Difference(dul, crop.LL);
    }

    /// <summary>Get the difference between two arrays of values.</summary>
    /// <param name="values1">The first array of values.</param>
    /// <param name="values2">The second array of values.</param>
    /// <returns>The difference between the two arrays.</returns>
    private static double[] Difference(IList<double> values1, IList<double> values2)
    {
        double[] difference = new double[values1.Count];
        for (int i = 0; i < values1.Count; i++)
            difference[i] += Math.Abs(values1[i] - values2[i]);
        return difference;
    }

    /// <summary>Map values to a thickness.</summary>
    /// <param name="values">The values to map.</param>
    /// <param name="thickness">The thickness to map to.</param>
    /// <returns>The mapped values.</returns>
    private static double[] MappedTo(this IList<double> values, IList<double> fromThickness, IList<double> toThickness)
    {
        return APSIM.Shared.Utilities.SoilUtilities.MapConcentration(values.ToArray(), fromThickness.ToArray(), toThickness.ToArray(), values.Last());
    }

    /// <summary>Convert volumetric values to mm.</summary>
    /// <param name="values">The values to convert.</param>
    /// <param name="thickness">The thickness.</param>
    /// <returns>The values in mm.</returns>
    private static double[] ToMM(this IList<double> values, IList<double> thickness)
    {
        double[] mm = new double[values.Count];
        for (int i = 0; i < thickness.Count; i++)
            mm[i] += values[i] * thickness[i];
        return mm;
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
}