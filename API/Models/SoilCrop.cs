#nullable enable
using System.Xml.Serialization;

namespace API.Models;

public class SoilCrop
{
    [XmlIgnore]
    public int Id { get; set; }
    [XmlIgnore]
    public int WaterId { get; set; }   // foreign key property

    [XmlAttribute("name")]
    public string Name { get; set; } = null!;
    public double[] Thickness { get; set; } = null!;
    public double[] LL { get; set; } = null!;
    public double[] KL { get; set; } = null!;
    public double[]? XF { get; set; } = null;
    public string[]? LLMetadata { get; set; } = null;
}
