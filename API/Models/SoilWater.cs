using System.Collections.Generic;

namespace API.Models;
public class SoilWater
{
    public int Id { get; set; }
    public double SummerCona { get; set; }
    public double SummerU { get; set; }
    public string SummerDate { get; set; }
    public double WinterCona { get; set; }
    public double WinterU { get; set; }
    public string WinterDate { get; set; }
    public double DiffusConst { get; set; }
    public double DiffusSlope { get; set; }
    public double Salb { get; set; }
    public int CN2Bare { get; set; }
    public int CNRed { get; set; }
    public double CNCov { get; set; }
    public double Slope { get; set; }
    public double DischargeWidth { get; set; }
    public double CatchmentArea { get; set; }
    public double MaxPond { get; set; }
    public List<double> Thickness { get; set; }
    public List<double> SWCON { get; set; }
}
