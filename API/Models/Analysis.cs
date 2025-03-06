using System.Collections.Generic;

namespace API.Models;

public class Analysis
{
    public int Id { get; set; }
    public List<double> Thickness { get; set; }
    public List<double> Rocks { get; set; }
    public List<string> RocksMetadata { get; set; }
    public List<string> Texture { get; set; }
    public List<string> TextureMetadata { get; set; }
    public List<double> PH { get; set; }
    public List<string> PHMetadata { get; set; }
    public List<double> CEC { get; set; }
    public List<string> CECMetadata { get; set; }
    public List<double> ParticleSizeSand { get; set; }
    public List<string> ParticleSizeSandMetadata { get; set; }
    public List<double> ParticleSizeSilt { get; set; }
    public List<string> ParticleSizeSiltMetadata { get; set; }
    public List<double> ParticleSizeClay { get; set; }
    public List<string> ParticleSizeClayMetadata { get; set; }
    public string PHUnits { get; set; }
    public string BoronUnits { get; set; }
}
