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
    public static IResult Add(SoilDbContext context, Models.Soil[] soils)
    {
        try
        {
            foreach (var soil in soils)
                AddOrUpdateSoil(context, soil);

            context.SaveChanges();
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.ToString());
        }
    }

    /// <summary>
    /// Add soils (using old APSoil folder format) to the database or update them if they already exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="filePath">The path to the XML file.</param>
    /// <returns>Results.Ok if success, otherwise Results.Problem</returns>
    public static IResult AddOld(SoilDbContext context, Models.Folder soilsFolder)
    {
        try
        {
            foreach (var soil in GetSoilsRecursively(soilsFolder))
                AddOrUpdateSoil(context, soil);

            context.SaveChanges();
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.ToString());
        }
    }

    /// <summary>Get all soils</summary>
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

        return context.Soils.ToArray();
    }

    /// <summary>Add or update a soil in the database.</summary>
    /// <param name="context">The database context.</param>
    /// <param name="soil">The soil to add or update.</param>
    private static void AddOrUpdateSoil(SoilDbContext context, Models.Soil soil)
    {
        if (context.Soils.Any(s => s.FullName == soil.FullName))
            context.Soils.Update(soil);
        else
            context.Soils.Add(soil);
    }

    /// <summary>Get all folders and subfolders recursively.</summary>
    /// <param name="folder">The root folder.</param>
    /// <returns>All folders and subfolders.</returns>
    private static IEnumerable<Models.Soil> GetSoilsRecursively(Models.Folder folder, string parentFolderPath = null)
    {
        foreach (var subFolder in folder.Folders)
            GetSoilsRecursively(subFolder, $"{parentFolderPath}/{folder.Name}");

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