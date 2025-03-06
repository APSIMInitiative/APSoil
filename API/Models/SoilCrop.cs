using System.Collections.Generic;

namespace API.Models;

public class SoilCrop
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<double> Thickness { get; set; }
    public List<double> LL { get; set; }
    public List<double> KL { get; set; }
    public List<double> XF { get; set; }
    public List<string> LLMetadata { get; set; }
}
