using System.Collections.Generic;

namespace SoilAPI.Models;

public class Water
{
    public List<double> Thickness { get; set; }
    public List<double> BD { get; set; }
    public List<double> AirDry { get; set; }
    public List<double> LL15 { get; set; }
    public List<double> DUL { get; set; }
    public List<double> SAT { get; set; }
    public List<double> KS { get; set; }
    public List<string> BDMetadata { get; set; }
    public List<string> AirDryMetadata { get; set; }
    public List<string> LL15Metadata { get; set; }
    public List<string> DULMetadata { get; set; }
    public List<string> SATMetadata { get; set; }
    public List<SoilCrop> SoilCrops { get; set; }
}
