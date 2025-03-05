using System.Collections.Generic;

namespace SoilAPI.Models;

public class SoilCrop
{
    public string Name { get; set; }
    public List<double> Thickness { get; set; }
    public List<double> LL { get; set; }
    public List<double> KL { get; set; }
    public List<double> XF { get; set; }
    public List<string> LLMetadata { get; set; }
}
