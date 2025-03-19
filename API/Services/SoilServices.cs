using System.Globalization;
using System.Text.Json;
using System.Xml.Serialization;
using API.Data;
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
            if (context.Soils.Any(s => s.FullName == soil.FullName))
                context.Soils.Update(soil);
            else
                context.Soils.Add(soil);
        }
        context.SaveChanges();
    }

    /// <summary>Get soils</summary>
    /// <param name="context">The database context.</param>
    public static Models.Soil[] Get(SoilDbContext context, string name = null, string folder = null, string soilType = null,
                                                           double latitude = double.NaN, double longitude = double.NaN)
    {
        IQueryable<Models.Soil> soils = context.Soils;
        if (name != null)
            soils = soils.Where(s => s.Name.Contains(name));
        if (folder != null)
            soils = soils.Where(s => s.FullName.Contains(folder));
        if (soilType != null)
            soils = soils.Where(s => s.SoilType.Contains(soilType));

        if (!double.IsNaN(latitude) && !double.IsNaN(longitude))
            soils = soils.OrderByDescending(s => Distance(s, latitude, longitude))
                         .Take(10);

        // return full soil, forcing eager loading of related entities.
        return soils.Include(s => s.Analysis)
                    .Include(s => s.SoilOrganicMatter)
                    .Include(s => s.SoilWater)
                    .Include(s => s.Water)
                    .Include(s => s.Water.SoilCrops)
                    .ToArray();
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
    public static string ToXML(this Models.Soil[] soils)
    {
        var folder = new Models.Folder();
        folder.Soils = [];
        folder.Soils.AddRange(soils);

        // Serialize the object synchronously then rewind the stream
        XmlSerializer Serializer = new(folder.GetType());
        using var textWriter = new StringWriter();
        Serializer.Serialize(textWriter, folder);
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

    /// <summary>Get all folders and subfolders recursively.</summary>
    /// <param name="folder">The root folder.</param>
    /// <returns>All folders and subfolders.</returns>
    private static IEnumerable<Models.Soil> GetSoilsRecursively(Models.Folder folder, string parentFolderPath = null)
    {
        foreach (var subFolder in folder.Folders)
           foreach (var soil in GetSoilsRecursively(subFolder, $"{parentFolderPath}/{folder.Name}"))
                yield return soil;

        foreach (var soil in folder.Soils)
        {
            soil.FullName = $"{parentFolderPath}/{soil.Name}";
            yield return soil;
        }
    }

    /// <summary>Get the distance between a soil and a point.</summary>
    /// <param name="soil">The soil.</param>
    /// <param name="latitude">The latitude of the point.</param>
    /// <param name="longitude">The longitude of the point.</param>
    /// <returns>The distance between the soil and the point.</returns>
    private static double Distance(Models.Soil soil, double latitude, double longitude)
    {
        return Math.Sqrt(Math.Pow(soil.Latitude - latitude, 2) + Math.Pow(soil.Longitude - longitude, 2));
    }
}