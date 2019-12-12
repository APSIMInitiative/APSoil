using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apsoil;
using System.Drawing;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Apsoil.Service service = new Service();
            byte[] b = service.SoilChartWithSamplePNG("Soils/Australia/South Australia/Mid North/Sandy loam over light-medium clays (Roseworthy No300)",
                                                       new double[] { 100.0, 300.0, 300.0, 300.0 },
                                                       new double[] { 0.01, 0.25, 0.18, 0.2 },
                                                       false);

            MemoryStream s = new MemoryStream(b);
            Bitmap bitmap = new Bitmap(s);
            bitmap.Save(@"C:\Users\hol353\Desktop\Test.png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
