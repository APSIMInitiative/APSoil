using System.Xml.Serialization;

namespace API.Models;

public class Folder
{
    public int Id { get; set; }

    [XmlAttribute("name")]
    public string Name { get; set; }
    public List<Folder> Folders { get; set; }
    public List<Soil> Soils { get; set; }
}
