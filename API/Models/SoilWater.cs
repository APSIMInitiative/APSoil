#nullable enable

using System.Xml.Serialization;

namespace API.Models;
public class SoilWater
{
    [XmlIgnore]
    public int Id { get; set; }
    [XmlIgnore]
    public int SoilId { get; set; }   // foreign key property

    public double SummerCona { get; set; }
    public double SummerU { get; set; }
    public string SummerDate { get; set; } = null!;
    public double WinterCona { get; set; }
    public double WinterU { get; set; }
    public string WinterDate { get; set; } = null!;
    public double DiffusConst { get; set; }
    public double DiffusSlope { get; set; }
    public double Salb { get; set; }
    public int CN2Bare { get; set; }
    public int CNRed { get; set; }
    public double CNCov { get; set; }
    public double? Slope { get; set; }
    public double? DischargeWidth { get; set; }
    public double? CatchmentArea { get; set; }
    public double? MaxPond { get; set; }
    public List<double> Thickness { get; set; } = null!;
    public List<double> SWCON { get; set; } = null!;
}
