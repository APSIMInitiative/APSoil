#nullable enable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace API.Models;
public class SoilOrganicMatter
{
    [XmlIgnore]
    public int Id { get; set; }
    [XmlIgnore]
    public int SoilId { get; set; }   // foreign key property
    public int RootCN { get; set; }
    public int RootWt { get; set; }
    public double SoilCN { get; set; }
    public double EnrACoeff { get; set; }
    public double EnrBCoeff { get; set; }
    public List<double> Thickness { get; set; } = null!;
    public List<double> OC { get; set; } = null!;
    public List<string>? OCMetadata { get; set; }
    public List<double> FBiom { get; set; } = null!;
    public List<double> FInert { get; set; } = null!;
    public string? OCUnits { get; set; }
}
