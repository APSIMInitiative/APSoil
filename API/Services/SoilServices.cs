using API.Data;
using API.Models;
using APSIM.Numerics;
using APSIM.Soils;
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
    /// <param name="recordNumber">The record number of the soil.</param>
    /// <param name="cropName">The name of the crop to use when cll or pawc is provided.</param>
    /// <param name="thickness">The thickness of the cll or PAWC values.</param>
    /// <param name="cll">The crop lower limit (volumetric).</param>
    /// <param name="cllIsGrav">True if the cll is gravimetric.</param>
    /// <param name="pawc">The plant available water capacity by layer (mm).</param>
    /// <returns>An array of full soil names.</returns>
    public static SoilsFromDb Search(SoilDbContext context, string name = null, string folder = null, string soilType = null, string country = null,
                                     double latitude = double.NaN, double longitude = double.NaN, double radius = double.NaN,
                                     string fullName = null,
                                     int recordNumber = int.MinValue,
                                     string cropName = null, double[] thickness = null, double[] cll = null, bool cllIsGrav = false, double[] pawc = null,
                                     int numToReturn = 0)
    {
        IQueryable<Models.Soil> soils = context.Soils;
        if (name != null)
            soils = soils.Where(s => s.Name.Trim().Contains(name));
        if (fullName != null)
            soils = soils.Where(s => s.FullName.Trim() == fullName);
        if (folder != null)
            soils = soils.Where(s => s.FullName.Contains(folder));
        if (soilType != null)
            soils = soils.Where(s => s.SoilType.Contains(soilType));
        if (country != null)
            soils = soils.Where(s => s.Country == country);
        if (cropName != null)
            soils = soils.Where(s => s.Water.SoilCrops.Any(sc => sc.Name == cropName.ToLower()));
        if (recordNumber != int.MinValue)
            soils = soils.Where(s => s.RecordNumber == recordNumber);

        // The above where clauses can be converted to SQL by EntityFramework.
        // The lat/long, cll and pawc orderby clauses below cannot be converted to SQL so must be done in memory by LINQ
        if (double.IsNaN(latitude) && double.IsNaN(longitude) && cll == null && pawc == null)
        {
            // return only the required number of soils.
            if (numToReturn > 0)
                soils = soils.Take(numToReturn);
            return new API.Services.SoilsFromDb(soils);
        }
        else
        {
            // If there are performance issues with this code, some fields could be added to soil
            // for pawc and cll for predefined layers (e.g. 0-300, 300-600, 600-900 etc) to allow
            // the query to be done in SQL (rather than needing a 'Difference' function).
            IEnumerable<Models.Soil> soilsInMemory = soils.Include(s => s.Water)
                                                          .Include(s => s.Water.SoilCrops)
                                                          .Include(s => s.SoilOrganicMatter)
                                                          .Include(s => s.SoilWater)
                                                          .Include(s => s.Analysis)
                                                          .ToList();

            if (!double.IsNaN(latitude) && !double.IsNaN(longitude))
            {
                if (!double.IsNaN(radius))
                    soilsInMemory = soilsInMemory.Where(s => MathUtilities.Distance(s.Latitude, s.Longitude, latitude, longitude) <= radius);
                soilsInMemory = soilsInMemory.OrderBy(s => MathUtilities.Distance(s.Latitude, s.Longitude, latitude, longitude));
            }

            if (cll != null)
            {
                if (cllIsGrav)
                    soilsInMemory = soilsInMemory.OrderBy(s => cll.Subtract(s.Crop(cropName)
                                                                             .LL
                                                                             .ConvertVolumetricToGravimetric(s.Water.BD)
                                                                             .MappedTo(s.Water.Thickness, thickness))
                                                 .Sum());
                else
                    soilsInMemory = soilsInMemory.OrderBy(s => cll.Subtract(s.Crop(cropName)
                                                                             .LL
                                                                             .MappedTo(s.Water.Thickness, thickness))
                                                 .Sum());
            }

            if (pawc != null)
            {
                soilsInMemory = soilsInMemory.OrderBy(s => pawc.Subtract(s.Crop(cropName)
                                                                          .PAWC(s.Water.DUL)
                                                                          .Multiply(s.Water.Thickness)
                                                                          .MappedTo(s.Water.Thickness, thickness)
                                                                     )
                                                           .Sum());
            }
            // return only the required number of soils.
            if (numToReturn > 0)
                soilsInMemory = soilsInMemory.Take(numToReturn);

            return new SoilsFromDb(soilsInMemory);
        }
    }

    /// <summary>Calculate and return the PAWC of a soil</summary>
    /// <param name="context">The database context.</param>
    /// <param name="fullName">The full name of the soil.</param>
    /// <param name="cropName">The name of the crop. Can be null for DUL-SW.</param>
    /// <returns>The plant available water(mm).</returns>
    public static double PAWC(SoilDbContext context, string fullName, string cropName)
    {
        var soils = context.Soils.Where(s => s.FullName.Trim() == fullName);
        if (!soils.Any())
            throw new Exception($"Soil with full name {fullName} not found.");
        return soils.Include(s => s.Water)
                    .Include(s => s.Water.SoilCrops)
                    .Include(s => s.SoilOrganicMatter)
                    .Include(s => s.SoilWater)
                    .Include(s => s.Analysis)
                    .First()
                    .ToAPSIMSoil()
                    .PAWCmm(cropName)
                    .Sum();
    }

    /// <summary>Calculate and return the PAW of a soil</summary>
    /// <param name="context">The database context.</param>
    /// <param name="fullName">The full name of the soil.</param>
    /// <param name="cropName">The name of the crop. Can be null for DUL-SW.</param>
    /// <param name="thickness">The thickness of the layers.</param>
    /// <param name="sw">The soil water content.</param>
    /// /// <param name="swIsGrav">True if the soil water content is gravimetric.</param>
    /// <returns>The plant available water(mm).</returns>
    public static double PAW(SoilDbContext context, string fullName, string cropName, IReadOnlyList<double> thickness, IReadOnlyList<double> sw, bool swIsGrav)
    {
        var soils = context.Soils.Where(s => s.FullName.Trim() == fullName);
        if (!soils.Any())
            throw new Exception($"Soil with full name {fullName} not found.");
        var soil = soils.Include(s => s.Water)
                        .Include(s => s.Water.SoilCrops)
                        .Include(s => s.SoilOrganicMatter)
                        .Include(s => s.SoilWater)
                        .Include(s => s.Analysis)
                        .First();

        IReadOnlyList<double> ll = null;
        IReadOnlyList<double> xf = null;
        if (cropName != null)
        {
            var crop = soil.Water.SoilCrops.FirstOrDefault(c => c.Name == cropName);
            if (crop == null)
                throw new Exception($"Soil has no data for crop: {cropName}.");
            ll = crop.LL;
            xf = crop.XF;
        }
        else
        {
            ll = soil.Water.LL15;
            xf = null;
        }

        if (swIsGrav)
        {
            IReadOnlyList<double> bdMapped = soil.Water.BD.MappedTo(soil.Water.Thickness, thickness);
            sw = sw.ConvertGravimetricToVolumetric(bdMapped);
        }

        IReadOnlyList<double> swMapped = sw.SWMappedTo(thickness, soil.Water.Thickness, soil.Water.LL15, ll, soil.Water.DUL);
        var pawByLayer = SoilUtilities.CalcPAWC(soil.Water.Thickness, ll, swMapped, xf);
        return MathUtilities.Multiply(pawByLayer, soil.Water.Thickness).Sum();
    }


    /// <summary>Map values to a thickness using a ramp down (0.8, 0.4, 0 x values.Last()) below profile. </summary>
    /// <param name="values">The values to map.</param>
    /// <param name="thickness">The thickness to map to.</param>
    /// <returns>The mapped values.</returns>
    public static double[] SWMappedTo(this IReadOnlyList<double> values, IReadOnlyList<double> fromThickness, IReadOnlyList<double> toThickness, IReadOnlyList<double> ll15, IReadOnlyList<double> ll, IReadOnlyList<double> dul)
    {
        List<double> sw = values.ToList();
        List<double> thickness = fromThickness.ToList();
        sw.Add(ll15.Last());     // add a pseudo layer below profile.
        thickness.Add(3000);
        var swMM = sw.Multiply(thickness);
        var swVolumetric = SoilUtilities.MapMass(swMM, thickness.ToArray(), toThickness.ToArray())
                            .Divide(toThickness);     // convert back to volumetric
        return swVolumetric.LowerConstraint(ll15, startIndex: 0)
                           .UpperConstraint(dul, startIndex: 0)
                           .ToArray();
    }

}