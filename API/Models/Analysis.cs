#nullable enable
using System.Xml.Serialization;

namespace API.Models;

public class Analysis
{
    [XmlIgnore]
    public int Id { get; set; }
    [XmlIgnore]
    public int SoilId { get; set; }   // foreign key property
    public double[] Thickness { get; set; } = null!;
    public double[]? Rocks { get; set; } = null;
    public string[]? RocksMetadata { get; set; } = null;
    public string[]? Texture { get; set; } = null;
    public string[]? TextureMetadata { get; set; } = null;
    public double[] PH { get; set; } = null!;
    public string[]? PHMetadata { get; set; } = null;
    public double[]? EC { get; set; } = null;
    public string[]? ECMetadata { get; set; } = null;
    public double[]? CL { get; set; } = null;
    public string[]? CLMetadata { get; set; } = null;
    public double[]? CEC { get; set; } = null;
    public string[]? CECMetadata { get; set; } = null;
    public double[]? ESP { get; set; } = null;
    public string[]? ESPMetadata { get; set; } = null;
    public double[]? ParticleSizeSand { get; set; } = null;
    public string[]? ParticleSizeSandMetadata { get; set; } = null;
    public double[]? ParticleSizeSilt { get; set; } = null;
    public string[]? ParticleSizeSiltMetadata { get; set; } = null;
    public double[]? ParticleSizeClay { get; set; } = null;
    public string[]? ParticleSizeClayMetadata { get; set; } = null;
    public string? PHUnits { get; set; } = null;
    public string? BoronUnits { get; set; } = null;
}
