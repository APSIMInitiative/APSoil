
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// A class that represents a collection of soils returned from SoilServices.Search.
/// </summary>
public class SoilsFromDb
{
    private IQueryable<Models.Soil> soilQuery;

    private IEnumerable<Models.Soil> soils;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoilsFromDb"/> class.
    /// </summary>
    /// <param name="soilQuery">The soil query.</param>
    public SoilsFromDb(IQueryable<Models.Soil> soilQuery)
    {
        this.soilQuery = soilQuery;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoilsFromDb"/> class.
    /// </summary>
    /// <param name="soilQuery">A collection of soils.</param>
    public SoilsFromDb(IEnumerable<Models.Soil> soils)
    {
        this.soils = soils;
    }

    /// <summary>
    /// The different output formats for the soils.
    /// </summary>
    public enum OutputFormatEnum { Names, BasicInfo, ExtendedInfo, FullSoil, KML }

    /// <summary>
    /// Converts the soils to a custom result in XML format.
    /// </summary>
    public IResult ToXMLResult(OutputFormatEnum outputFormat)
    {
        object obj;
        if (outputFormat == OutputFormatEnum.BasicInfo)
            obj = soilQuery == null ? soils.Select(soil => soil.ToBasicInfo()).ToArray() : soilQuery.Select(soil => soil.ToBasicInfo()).ToArray();
        else if (outputFormat == OutputFormatEnum.ExtendedInfo)
            obj = ToExtendedInfo();
        else if (outputFormat == OutputFormatEnum.FullSoil)
            obj = ToSoils().ToFolder();
        else if (outputFormat == OutputFormatEnum.KML)
            return Results.File(ToSoils().ToRecursiveFolder().ToKMZ(), "application/vnd.google-earth.kmz", "soils.kmz");
        else
            obj = soilQuery == null ? soils.Select(soil => soil.FullName).ToArray() : soilQuery.Select(soil => soil.FullName).ToArray();

        return new CustomResult<string>(obj.ToXML(), "application/xml");
    }

    /// <summary>
    /// Converts the soils to an array of soils.
    /// </summary>
    public Models.Soil[] ToSoils()
    {
        return soilQuery == null ? soils.ToArray() : soilQuery.Include(s => s.Water)
                                                              .Include(s => s.Water.SoilCrops)
                                                              .Include(s => s.SoilOrganicMatter)
                                                              .Include(s => s.SoilWater)
                                                              .Include(s => s.Analysis)
                                                              .ToArray();
    }

    /// <summary>
    /// Converts the soils to an array of SoilInfos.
    /// </summary>
    private Models.SoilInfo[] ToExtendedInfo()
    {
        return soilQuery == null ? soils.Select(soil => soil.ToInfo()).ToArray() : soilQuery.Include(s => s.Water)
                                                                                            .Include(s => s.Water.SoilCrops)
                                                                                            .Include(s => s.Analysis)
                                                                                            .Select(soil => soil.ToInfo())
                                                                                            .ToArray();
    }
}
