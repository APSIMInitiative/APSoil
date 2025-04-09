using System.Xml.Serialization;

namespace API.Models;

public class Folder
{
    [XmlIgnore]
    public int Id { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlElement("Folder")]
    public List<Folder> Folders { get; set; }

    [XmlElement("Soil")]
    public List<Soil> Soils { get; set; }
}
