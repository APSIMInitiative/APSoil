using System.Collections.Generic;

namespace SoilAPI.Models;
public class SoilOrganicMatter
{
    public int RootCN { get; set; }
    public int RootWt { get; set; }
    public double SoilCN { get; set; }
    public double EnrACoeff { get; set; }
    public double EnrBCoeff { get; set; }
    public List<double> Thickness { get; set; }
    public List<double> OC { get; set; }
    public List<string> OCMetadata { get; set; }
    public List<double> FBiom { get; set; }
    public List<double> FInert { get; set; }
    public string OCUnits { get; set; }
}
