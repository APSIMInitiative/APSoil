#nullable enable
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

[Index(nameof(FullName), IsUnique = true)]
public class Soil
{
    [XmlIgnore]
    public int Id { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlIgnore]
    public string? FullName { get; set; }
    public int RecordNumber { get; set; }
    public string? ASCOrder { get; set; }
    public string? ASCSubOrder { get; set; }
    public string? SoilType { get; set; }
    public string? LocalName { get; set; }
    public string? Site { get; set; }
    public string? NearestTown { get; set; }
    public string? Region { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? NaturalVegetation { get; set; }
    public string? ApsoilNumber { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationAccuracy { get; set; }
    public int YearOfSampling { get; set; }
    public string? DataSource { get; set; }
    public string? Comments { get; set; }
    public Water Water { get; set; } = null!;
    public SoilWater? SoilWater { get; set; }
    public SoilOrganicMatter? SoilOrganicMatter { get; set; }
    public Analysis? Analysis { get; set; }
}
