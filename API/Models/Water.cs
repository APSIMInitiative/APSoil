#nullable enable
using System.Xml.Serialization;

namespace API.Models;

public class Water
{
    [XmlIgnore]
    public int Id { get; set; }
    [XmlIgnore]
    public int SoilId { get; set; }   // foreign key property
    public double[] Thickness { get; set; } = null!;
    public double[] BD { get; set; } = null!;
    public double[] AirDry { get; set; } = null!;
    public double[] LL15 { get; set; } = null!;
    public double[] DUL { get; set; } = null!;
    public double[] SAT { get; set; } = null!;
    public double[]? KS { get; set; } = null;
    public string[]? BDMetadata { get; set; } = null;
    public string[]? AirDryMetadata { get; set; } = null;
    public string[]? LL15Metadata { get; set; } = null;
    public string[]? DULMetadata { get; set; } = null;
    public string[]? SATMetadata { get; set; } = null;
    [XmlElement("SoilCrop")]
    public List<SoilCrop>? SoilCrops { get; set; } = null;
}
