namespace API.Models;
public class SoilInfo
{
    public string Name;
    public string Description;
    public string SoilType;
    public double Latitude;
    public double Longitude;
    public double Distance;
    public string ASCOrder;
    public string ASCSubOrder;
    public string Site;
    public string Region;
    public string NearestTown;
    public double[] Thickness;
    public string[] Texture;
    public double[] EC;
    public double[] PH;
    public double[] CL;
    public double[] Boron;
    public double[] ESP;
    public double[] AL;
    public double[] LL15;
    public double[] DUL;
    public List<SoilCropInfo> Crops;
}
public class SoilCropInfo
{
    public string Name;
    public double[] LL;
    public double[] KL;
    public double[] XF;
}