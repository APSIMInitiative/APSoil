#nullable enable
using System.Xml.Serialization;

namespace API.Models;

public class Water
{
    [XmlIgnore]
    public int Id { get; set; }
    [XmlIgnore]
    public int SoilId { get; set; }   // foreign key property
    public List<double> Thickness { get; set; } = null!;
    public List<double> BD { get; set; } = null!;
    public List<double> AirDry { get; set; } = null!;
    public List<double> LL15 { get; set; } = null!;
    public List<double> DUL { get; set; } = null!;
    public List<double> SAT { get; set; } = null!;
    public List<double>? KS { get; set; } = null;
    public List<string>? BDMetadata { get; set; } = null;
    public List<string>? AirDryMetadata { get; set; } = null;
    public List<string>? LL15Metadata { get; set; } = null;
    public List<string>? DULMetadata { get; set; } = null;
    public List<string>? SATMetadata { get; set; } = null;
    [XmlElement("SoilCrop")]
    public List<SoilCrop>? SoilCrops { get; set; } = null;
}
