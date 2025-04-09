
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class SoilsFromDb
{
    private IQueryable<Models.Soil> soilQuery;

    private Models.Soil[] soils;

    public SoilsFromDb(IQueryable<Models.Soil> soilQuery)
    {
        this.soilQuery = soilQuery;
    }

    public SoilsFromDb(IEnumerable<Models.Soil> soils)
    {
        this.soils = soils.ToArray();
    }

    public enum OutputFormatEnum { Names, BasicInfo, ExtendedInfo, FullSoil }

    public IResult ToXMLResult(OutputFormatEnum outputFormat)
    {
        object obj;
        if (outputFormat == OutputFormatEnum.BasicInfo)
            obj = soilQuery == null ? soils.Select(soil => soil.ToBasicInfo()).ToArray() : soilQuery.Select(soil => soil.ToBasicInfo()).ToArray();
        else if (outputFormat == OutputFormatEnum.ExtendedInfo)
            obj = ToExtendedInfo();
        else if (outputFormat == OutputFormatEnum.FullSoil)
            obj = ToSoils().ToFolder();
        else
            obj = soilQuery == null ? soils.Select(soil => soil.FullName).ToArray() : soilQuery.Select(soil => soil.FullName).ToArray();

        return new CustomResult<string>(obj.ToXML(), "application/xml");
    }

    public Models.Soil[] ToSoils()
    {
        return soilQuery == null ? soils.ToArray() : soilQuery.Include(s => s.Water)
                                                              .Include(s => s.Water.SoilCrops)
                                                              .Include(s => s.SoilOrganicMatter)
                                                              .Include(s => s.SoilWater)
                                                              .Include(s => s.Analysis)
                                                              .ToArray();
    }


    public Models.SoilInfo[] ToExtendedInfo()
    {
        return soilQuery == null ? soils.Select(soil => soil.ToInfo()).ToArray() : soilQuery.Include(s => s.Water)
                                                                                            .Include(s => s.Water.SoilCrops)
                                                                                            .Include(s => s.Analysis)
                                                                                            .Select(soil => soil.ToInfo())
                                                                                            .ToArray();
    }
}
