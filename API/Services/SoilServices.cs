using API.Data;
using API.Models;
using APSIM.Shared.Utilities;
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
    /// <param name="cllIsGrav">True if the cll is gravimetric.</param>
    /// <param name="pawc">The plant available water capacity by layer (mm).</param>
    /// <returns>An array of full soil names.</returns>
    public static SoilsFromDb Search(SoilDbContext context, string name = null, string folder = null, string soilType = null, string country = null,
                                     double latitude = double.NaN, double longitude = double.NaN, double radius = double.NaN,
                                     string fullName = null,
                                     string cropName = null, double[] thickness = null, double[] cll = null, bool cllIsGrav = false, double[] pawc = null,
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
            return new API.Services.SoilsFromDb(soils);
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
            {
                if (cllIsGrav)
                    soilsInMemory = soilsInMemory.OrderBy(s => Difference(cll, s.Crop(cropName)
                                                                                .LL
                                                                                .ConvertGravimetricToVolumetric(s.Water.BD)
                                                                                .MappedTo(s.Water.Thickness, thickness))
                                                 .Sum());
                else
                    soilsInMemory = soilsInMemory.OrderBy(s => Difference(cll, s.Crop(cropName)
                                                                                .LL
                                                                                .MappedTo(s.Water.Thickness, thickness))
                                                 .Sum());

            }

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
        var soils = context.Soils.Where(s => s.FullName == fullName);
        if (!soils.Any())
            throw new Exception($"Soil with full name {fullName} not found.");
        var soil = soils.Include(s => s.Water)
                        .Include(s => s.Water.SoilCrops)
                        .Include(s => s.SoilOrganicMatter)
                        .Include(s => s.SoilWater)
                        .Include(s => s.Analysis)
                        .First();
        List<double> ll = null;
        List<double> xf = null;
        if (cropName != null)
        {
            var crop = soil.Water.SoilCrops.FirstOrDefault(c => c.Name == cropName);
            if (crop == null)
                throw new Exception($"Soil has no data for crop: {cropName}.");
            ll = crop.LL;
        }
        else
        {
            ll = soil.Water.LL15;
            xf = null;
        }
        var pawcByLayer = SoilUtilities.CalcPAWC(soil.Water.Thickness.ToArray(), ll.ToArray(), soil.Water.DUL.ToArray(), xf?.ToArray());
        var pawc = MathUtilities.Multiply(pawcByLayer, soil.Water.Thickness.ToArray());
        return pawc.Sum();
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
        var soils = context.Soils.Where(s => s.FullName == fullName);
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

        IReadOnlyList<double> swMapped = sw.SWMappedTo(thickness, soil.Water.Thickness, ll);
        if (swIsGrav)
            swMapped = swMapped.ConvertGravimetricToVolumetric(soil.Water.BD);
        var pawByLayer = SoilUtilities.CalcPAWC(soil.Water.Thickness, ll, swMapped, xf);
        return MathUtilities.Multiply(pawByLayer, soil.Water.Thickness).Sum();
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
    private static double[] MappedTo(this IReadOnlyList<double> values, IReadOnlyList<double> fromThickness, IReadOnlyList<double> toThickness)
    {
        return APSIM.Shared.Utilities.SoilUtilities.MapConcentration(values.ToArray(), fromThickness.ToArray(), toThickness.ToArray());
    }

    /// <summary>Map values to a thickness using a ramp down (0.8, 0.4, 0 x values.Last()) below profile.</summary>
    /// <param name="values">The values to map.</param>
    /// <param name="thickness">The thickness to map to.</param>
    /// <returns>The mapped values.</returns>
    private static double[] SWMappedTo(this IReadOnlyList<double> values, IReadOnlyList<double> fromThickness, IReadOnlyList<double> toThickness, IReadOnlyList<double> ll)
    {
        List<double> sw = values.ToList();
        List<double> thickness = fromThickness.ToList();
        sw.Add(0.8 * values.Last());     // 1st pseudo layer below profile.
        sw.Add(0.4 * values.Last());     // 2nd pseudo layer below profile.
        sw.Add(0.0);                     // 3rd pseudo layer below profile.
        thickness.Add(thickness.Last()); // 1st pseudo layer below profile.
        thickness.Add(thickness.Last()); // 2nd pseudo layer below profile.
        thickness.Add(3000);             // 3rd pseudo layer below profile.

        var llMapped = ll.MappedTo(toThickness, thickness);
        var swMM = sw.LowerConstraint(llMapped, startIndex: values.Count)
                     .Multiply(thickness);
        return SoilUtilities.MapMass(swMM, thickness.ToArray(), toThickness.ToArray())
                            .Divide(toThickness);     // convert back to volumetric
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
}