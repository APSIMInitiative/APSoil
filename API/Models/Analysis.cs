#nullable enable
using System.Xml.Serialization;

namespace API.Models;

public class Analysis
{
    [XmlIgnore]
    public int Id { get; set; }
    [XmlIgnore]
    public int SoilId { get; set; }   // foreign key property
    public List<double> Thickness { get; set; } = null!;
    public List<double>? Rocks { get; set; } = null;
    public List<string>? RocksMetadata { get; set; } = null;
    public List<string>? Texture { get; set; } = null;
    public List<string>? TextureMetadata { get; set; } = null;
    public List<double> PH { get; set; } = null!;
    public List<string>? PHMetadata { get; set; } = null;
    public List<double>? CEC { get; set; } = null;
    public List<string>? CECMetadata { get; set; } = null;
    public List<double>? ParticleSizeSand { get; set; } = null;
    public List<string>? ParticleSizeSandMetadata { get; set; } = null;
    public List<double>? ParticleSizeSilt { get; set; } = null;
    public List<string>? ParticleSizeSiltMetadata { get; set; } = null;
    public List<double>? ParticleSizeClay { get; set; } = null;
    public List<string>? ParticleSizeClayMetadata { get; set; } = null;
    public string? PHUnits { get; set; } = null;
    public string? BoronUnits { get; set; } = null;
}
