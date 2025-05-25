using System.Xml.Serialization;
using APSIM.Numerics;
using APSIM.Soils;
using APSIM.Graphs;
using System.Text;

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
        using var textWriter = new Utf8StringWriter();
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
    /// Convert a collection of bytes to an IResult
    /// </summary>
    /// <param name="text">The text string.</param>
    /// <returns>A custom IResult.</returns>
    public static IResult ToImageResult(this byte[] bytes)
    {
        return new CustomResult<byte[]>(bytes, "image/png");
    }

    /// <summary>
    /// Convert an array of soils to a Folder.
    /// </summary>
    /// <param name="soils">The array of soils.</param>
    /// <returns>An XML string.</returns>
    public static API.Models.Folder ToFolder(this API.Models.Soil[] soils)
    {
        return new Models.Folder()
        {
            Name = "Soils",
            Soils = soils.ToList()
        };
    }

    /// <summary>
    /// Convert an array of soils to a nested Folder.
    /// </summary>
    /// <param name="soils">The array of soils.</param>
    /// <returns>An XML string.</returns>
    public static API.Models.Folder ToRecursiveFolder(this API.Models.Soil[] soils, string folderName = "Soils")
    {
        var folder = new Models.Folder()
        {
            Name = "Soils",
            Folders = new List<Models.Folder>(),
            Soils = new List<Models.Soil>()
        };

        foreach (var soil in soils)
            InsertSoilIntoFolder(soil, folder);

        return folder;
    }

    private static void InsertSoilIntoFolder(Models.Soil soil, Models.Folder folder)
    {
        var paths = soil.FullName.Split('/');
        Models.Folder f = folder;
        foreach (var path in paths.Skip(1)) // Skip the first element which is the root folder name
        {
            if (path != paths.Last())
            {
                // Must be a folder - create it if it doesn't exist
                f = folder.Folders.FirstOrDefault(f => f.Name == path);
                if (f == null)
                {
                    f = new Models.Folder()
                    {
                        Name = path,
                        Folders = new List<Models.Folder>(),
                        Soils = new List<Models.Soil>()
                    };
                    folder.Folders.Add(f);
                }
                folder = f;
            }
            else
            {
                // Must be a soil - add it to the folder
                f.Soils.Add(soil);
            }
        }
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
            Thickness = soil.Water?.Thickness,
            Texture = soil.Analysis?.Texture,
            DUL = soil.Water?.DUL,
            LL15 = soil.Water?.LL15,
            EC = soil.Analysis?.EC,
            CL = soil.Analysis?.CL,
            ESP = soil.Analysis?.ESP,
            PH = soil.Analysis?.PH,
            Crops = soil.Water?.SoilCrops?.Select(c => new Models.SoilCropInfo
            {
                Name = c.Name,
                LL = c.LL,
                KL = c.KL,
                XF = c.XF
            }).ToList()
        };
    }

    /// <summary>
    /// Convert a soil to a soil graph.
    /// </summary>
    /// <param name="soil"></param>
    /// <returns></returns>
    public static Graph ToGraph(this Models.Soil soil, double[] thickness = null,
                                double[] sw = null, bool swIsGrav = false,
                                string cropName = null)
    {
        IReadOnlyList<double> ll;
        IReadOnlyList<double> xf;
        if (cropName != null)
        {
            var crop = soil.Crop(cropName);
            ll = crop.LL;
            xf = crop.XF;
        }
        else
        {
            ll = soil.Water.LL15;
            xf = Enumerable.Repeat(1.0, ll.Count).ToArray();
        }

        double paw = double.NaN;
        if (sw != null)
        {
            if (swIsGrav)
            {
                var bdMapped = soil.Water.BD.MappedTo(soil.Water.Thickness, thickness);
                sw = sw.ConvertGravimetricToVolumetric(bdMapped).ToArray();
            }
            // Map water to bottom of profile.
            sw = Soil.SWMappedTo(sw, thickness, soil.Water.Thickness, ll);

            // Calculate plant available water.
            paw = SoilUtilities.CalcPAWC(soil.Water.Thickness, ll, sw, xf)
                               .Multiply(soil.Water.Thickness)
                               .Sum();
        }
        // Calculate PAWC for graph.
        double pawc = SoilUtilities.CalcPAWC(soil.Water.Thickness, ll, soil.Water.DUL, Enumerable.Repeat(1.0, ll.Count).ToArray())
                                   .Multiply(soil.Water.Thickness).Sum();

        return SoilGraph.Create(soil.Name, soil.Water.Thickness.ToMidPoints(), soil.Water.AirDry, soil.Water.LL15,
                                    soil.Water.DUL, soil.Water.SAT, ll, cropName, pawc, paw, thickness?.ToMidPoints(), sw);
    }

    /// <summary>Get the crop lower limit (volumetric) for a soil.</summary>
    /// <param name="cropName">The crop name.</param>
    /// <returns>The crop lower limit.</returns>
    public static Models.SoilCrop Crop(this Models.Soil soil, string cropName)
    {
        if (soil.Water == null || soil.Water.SoilCrops == null)
            throw new Exception("Soil has no soilcrop data.");
        var crop = soil.Water.SoilCrops.FirstOrDefault(c => c.Name == cropName);
        if (crop == null)
            throw new Exception($"Soil has no soilcrop data for {cropName}.");
        return crop;
    }

    /// <summary>
    /// Get the plant available water content for a soil crop.
    /// </summary>
    /// <param name="crop"></param>
    /// <param name="dul"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static double[] PAWC(this Models.SoilCrop crop, IList<double> dul)
    {
        if (crop.LL == null)
            throw new Exception($"SoilCrop {crop.Name} has no LL data.");
        return dul.Subtract(crop.LL);
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
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

}