using Apsoil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var apsoil = new Service();
            var s = apsoil.SoilChartPNG("Soils/Australia/Queensland/Darling Downs and Granite Belt/Grey Vertosol (Goondiwindi No862)");

            using (var fs = new FileStream(@"C:\Users\hol353\Temp\WebSites\test.png", FileMode.Create, FileAccess.Write))
            {
                fs.Write(s, 0, s.Length);
            }
        }
    }
}
