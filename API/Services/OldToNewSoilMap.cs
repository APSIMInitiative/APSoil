using AutoMapper;

namespace API.Services;

public static class DataToDomainModelMap
{
    private static IMapper mapper;
    public static APSIM.Soils.Soil ToAPSIMSoil(this Models.Soil soil)
    {
        if (mapper == null)
        {
            // https://dotnettutorials.net/lesson/ignore-using-automapper-in-csharp/
            // https://docs.automapper.org/en/stable/Value-converters.html
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Models.Water, APSIM.Soils.Physical>();
                cfg.CreateMap<Models.SoilCrop, APSIM.Soils.SoilCrop>();
                cfg.CreateMap<Models.SoilOrganicMatter, APSIM.Soils.Organic>()
                   .ForMember(dest => dest.SoilCNRatio, act => act.ConvertUsing(new DoubleToDoubleArray(), src => src.SoilCN))
                   .ForMember(dest => dest.FOM, act => act.ConvertUsing(new DoubleToDoubleArray(), src => src.RootWt))
                   .ForMember(dest => dest.FOMCNRatio, act => act.MapFrom(src => src.RootCN))
                   .ForMember(dest => dest.Carbon, act => act.MapFrom(src => src.OC))
                   .ForMember(dest => dest.CarbonMetadata, act => act.MapFrom(src => src.OCMetadata))
                   .ForMember(dest => dest.CarbonUnits, act => act.MapFrom(src => src.OCUnits));
                cfg.CreateMap<Models.Analysis, APSIM.Soils.Chemical>();
                cfg.CreateMap<Models.SoilWater, APSIM.Soils.WaterBalance>();
                cfg.CreateMap<Models.Soil, APSIM.Soils.Soil>()
                   .ForMember(dest => dest.Physical, act => act.MapFrom(src => src.Water))
                   .ForMember(dest => dest.Organic, act => act.MapFrom(src => src.SoilOrganicMatter))
                   .ForMember(dest => dest.WaterBalance, act => act.MapFrom(src => src.SoilWater))
                   .ForMember(dest => dest.Chemical, act => act.MapFrom(src => src.Analysis));
            });

            mapper = config.CreateMapper();
        }
        var apsimSoil = mapper.Map<APSIM.Soils.Soil>(soil);

        // Some manual mapping. Can't find a way for AutoMapper to do it.
        apsimSoil.Physical.Rocks = soil.Analysis.Rocks?.ToList();
        apsimSoil.Physical.RocksMetadata = soil.Analysis.RocksMetadata?.ToList();
        apsimSoil.Physical.Texture = soil.Analysis.Texture?.ToList();
        apsimSoil.Physical.TextureMetadata = soil.Analysis.TextureMetadata?.ToList();
        apsimSoil.Physical.ParticleSizeClay = soil.Analysis.ParticleSizeClay?.ToList();
        apsimSoil.Physical.ParticleSizeClayMetadata = soil.Analysis.ParticleSizeClayMetadata?.ToList();
        apsimSoil.Physical.ParticleSizeSand = soil.Analysis.ParticleSizeSand?.ToList();
        apsimSoil.Physical.ParticleSizeSandMetadata = soil.Analysis.ParticleSizeSandMetadata?.ToList();
        apsimSoil.Physical.ParticleSizeSilt = soil.Analysis.ParticleSizeSilt?.ToList();
        apsimSoil.Physical.ParticleSizeSiltMetadata = soil.Analysis.ParticleSizeSiltMetadata?.ToList();
        return apsimSoil;
    }

    public class DoubleToDoubleArray : IValueConverter<double, double[]> {
        public double[] Convert(double source, ResolutionContext context)
            => Enumerable.Repeat(source, 10).ToArray();
    }
}