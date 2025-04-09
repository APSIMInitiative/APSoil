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
    public List<double> Thickness { get; set; } = null!;
    public List<double> LL { get; set; } = null!;
    public List<double> KL { get; set; } = null!;
    public List<double> XF { get; set; } = null!;
    public List<string>? LLMetadata { get; set; } = null;
}
