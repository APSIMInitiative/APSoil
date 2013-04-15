using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Apsoil;
using Newtonsoft.Json.Linq;
using CSGeneral;
using System.Xml;
using Newtonsoft.Json;

[TestFixture]
public class Tests
{
    class Assert : NUnit.Framework.Assert
    {
        public static void AreEqual(double[] expected, double[] actual, double delta)
        {
            expected = MathUtility.RemoveMissingValuesFromBottom(expected);
            actual = MathUtility.RemoveMissingValuesFromBottom(actual);
            AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                AreEqual(expected[i], actual[i], delta);
            }
        }
    }

    //[Test]
    public void UpgradeSoils()
    {
        Service S = new Service();
        S.ConvertOldSoilsToNewSoils();
    }


    #region YP

    /// <summary>
    /// Usecase: Yield Prophet asks for a list of all soil names.
    /// </summary>
    [Test]
    public void YPSoilNames()
    {
        Service S = new Service();
        List<string> Names = S.AllSoilNames(IncludeUserSoils: true);

        Assert.Contains("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)", Names);
        Assert.Contains("Soils/Australia/South Australia/Mid North/Loam over light-medium-light clays (CM026)(Wheatlands No288) ", Names);

        int NumUserSoils = 0;
        for (int i = 0; i < Names.Count; i++)
            if (Names[i].Contains("/UserSoils/"))
                NumUserSoils++;

        Assert.Greater(NumUserSoils, 0);

        Names = S.SoilNames();
        Assert.Contains("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)", Names);
        Assert.Contains("Soils/Australia/South Australia/Mid North/Loam over light-medium-light clays (CM026)(Wheatlands No288) ", Names);

        NumUserSoils = 0;
        for (int i = 0; i < Names.Count; i++)
            if (Names[i].Contains("/UserSoils/"))
                NumUserSoils++;

        Assert.Greater(NumUserSoils, 0);
    }

    /// <summary>
    /// Usecase: Yield Prophet user views the XML of a soil ?????? 
    /// </summary>
    [Test]
    public void YPGetSoilXML()
    {
        Service S = new Service();
        string Xml = S.SoilXML("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)");

        // Make sure we can parse the XML
        XmlDocument Doc = new XmlDocument();
        Doc.LoadXml(Xml);

        Assert.AreEqual(XmlHelper.Name(Doc.DocumentElement), "Black Vertosol-Orion (Capella No049)");
    }

    /// <summary>
    /// Usecase: Yield Prophet converts an old format Sample XML into a new one.
    /// </summary>
    [Test]
    public void YPConvertSoilSampleXML()
    {
        Service S = new Service();

        // Make sure we can convert from old sample xml to new sample xml.
        string OldSoilSampleXML = "<soilsample name=\"Soil sample 1\">" +
                               "<profile>" +
                               "  <layer>" +
                               "    <thickness>100</thickness>" +
                               "    <no3>10.33</no3>" +
                               "    <nh4>2</nh4>" +
                               "    <sw>0.0689</sw>" +
                               "  </layer>" +
                               "  <layer>" +
                               "    <thickness>100</thickness>" +
                               "    <no3>9</no3>" +
                               "    <nh4>0</nh4>" +
                               "    <sw>0.1311</sw>" +
                               "  </layer>" +
                               "  <layer>" +
                               "    <thickness>200</thickness>" +
                               "    <no3>9</no3>" +
                               "    <nh4>0</nh4>" +
                               "    <sw>0.19</sw>" +
                               "  </layer>" +
                               "  <layer>" +
                               "    <thickness>200</thickness>" +
                               "    <no3>9</no3>" +
                               "    <nh4>0</nh4>" +
                               "    <sw>0.225</sw>" +
                               "  </layer>" +
                               "  <layer>" +
                               "    <thickness>200</thickness>" +
                               "    <no3>2.67</no3>" +
                               "    <nh4>0</nh4>" +
                               "    <sw>0.1726</sw>" +
                               "  </layer>" +
                               "</profile>" +
                               "</soilsample>";
        string NewSoilSampleXML = S.ConvertSoilSampleXML(OldSoilSampleXML);
        // Make sure we can parse the XML
        XmlDocument Doc = new XmlDocument();
        Doc.LoadXml(NewSoilSampleXML);
    }

    /// <summary>
    /// Usecase: Yield Prophet user selects a soil and displays a chart.
    /// </summary>
    [Test]
    public void YPViewSoilChart()
    {
        Service S = new Service();
        byte[] ChartBytes = S.SoilChartPNG("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)");
        ImageForm F = new ImageForm();
        F.SetImage(ChartBytes);
        F.ShowDialog();
    }

    /// <summary>
    /// Usecase: Yield Prophet user selects a soil and a sample and displays a chart.
    /// </summary>
    [Test]
    public void YPViewSoilChartWithSample()
    {
        Service S = new Service();

        double[] Thickness = new double[] { 100, 300, 300, 300 };
        double[] SW = new double[] { 0.139, 0.215, 0.179, 0.213 };  // not % but mm/mm - divide percent by 100
        double[] NO3 = new double[] { 9, 19, 9, 2 };
        double[] NH4 = new double[] { 6, 6, 6, 4 };
        byte[] ChartBytes = S.SoilChartWithSamplePNG("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)",
            Thickness, SW, IsGravimetric:true);

        ImageForm F = new ImageForm();
        F.SetImage(ChartBytes);
        F.ShowDialog();
    }

    /// <summary>
    /// Usecase: A YP user selects a soil and views the calculated PAWC.
    /// </summary>
    [Test]
    public void YPViewPAWC()
    {
        Service S = new Service();
        double PAWC = S.PAWC("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)", "Wheat");
        Assert.AreEqual(PAWC, 145.5);
    }

    /// <summary>
    /// Usecase: A YP user selects a soil and views the calculated wheat PAW.
    /// </summary>
    [Test]
    public void YPViewPAW()
    {
        Service S = new Service();

        double[] Thickness = new double[] { 100, 300, 300, 300 };
        double[] SW = new double[] { 0.056, 0.141, 0.209, 0.245 };  // not % but mm/mm - divide percent by 100
        double[] NO3 = new double[] { 9, 19, 9, 2 };
        double[] NH4 = new double[] { 6, 6, 6, 4 };

        double PAW = S.PAW("Soils/Australia/Victoria/Mallee/Clay Loam (Jil Jil No728)", 
                           Thickness, SW, IsGravimetric:true,
                           CropName:"Wheat");
        Assert.AreEqual(PAW, 145.5, 0.01);
    }

    /// <summary>
    /// Usecase: A YP user, after entering their latitude and longitude, view soil names within a radius of
    ///          50km. They then filter on soil type "Clay Loam".
    /// </summary>
    [Test]
    public void YPSearchSoils()
    {
        Service S = new Service();
        Apsoil.Service.SoilInfo[] MatchingSoils = S.SearchSoilsReturnInfo(-35.884, 142.983, 50, null);
        Assert.AreEqual(MatchingSoils.Count(), 26);

        // Now filter on clay loams
        MatchingSoils = S.SearchSoilsReturnInfo(-35.884, 142.983, 50, "Clay Loam");
        Assert.AreEqual(MatchingSoils.Count(), 15);
    }

    [Test]
    public void YPGetInfo()
    {
        Service S = new Service();

        Apsoil.Service.SoilInfo Info = S.GetSoilInfo("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)");

        Assert.AreEqual(Info.Name, "Black Vertosol-Orion (Capella No049)");
        Assert.AreEqual(Info.ASCOrder, "Vertosol");
        Assert.AreEqual(Info.ASCSubOrder, "Black");
        Assert.AreEqual(Info.Latitude, -22.962);
        Assert.AreEqual(Info.Longitude, 147.801);
        Assert.AreEqual(Info.NearestTown, "Capella, Q 4723");
        Assert.AreEqual(Info.Region, "Central Highlands");
        Assert.AreEqual(Info.Site, "Capella");
        Assert.AreEqual(Info.SoilType, "Clay");
    }

    [Test]
    public void YPMissingSampleData()
    {
        Service S = new Service();

        double[] Thickness = new double[] { 100, 300, 300, 3000 };
        double[] SW = new double[] { 0.139, 0.315, 999999, 999999 };  // not % but mm/mm - divide percent by 100

        double PAW = S.PAW("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)",
                           Thickness, SW, true, "wheat");
        Assert.AreEqual(PAW, 25.8, 0.01);
    }

    #endregion

    #region iPad app


    /// <summary>
    /// Usecase: iPad app is started and the user is shown a list of soil markers on a map of Australia.
    /// </summary>
    [Test]
    public void iPadAppStartApp()
    {
        Service S = new Service();
        Service.SoilBasicInfo[] Soils = S.AllAustralianSoils(new Service.SearchSoilsParams());
        Assert.Greater(Soils.Count(), 800);
    }

    /// <summary>
    /// Usecase: iPad app user selects a lat and long and is shown a selection of soils within range.
    /// </summary>
    [Test]
    public void iPadAppShowSoilsInRadius()
    {
        Service S = new Service();
        Service.SoilInfo[] Soils = S.SearchSoils(new Service.SearchSoilsParams() { Latitude = -28.0, Longitude = 151.0, Radius = 50 });
        Assert.AreEqual(Soils.Count(), 6);
        Assert.AreEqual(Soils[0].Name, "Soils/Australia/Queensland/Darling Downs and Granite Belt/Grey Vertosol-Box_Brigalow (Millwood No035)");
        Assert.AreEqual(Soils[0].Latitude, -28.003);
        Assert.AreEqual(Soils[0].Longitude, 151.169);

        // Filter on grey vertosols only.
        Soils = S.SearchSoils(new Service.SearchSoilsParams() { Latitude = -28.0, Longitude = 151.0, Radius = 50, 
                                                                ASCOrder = "Vertosol", ASCSubOrder = "Grey"});
        Assert.AreEqual(Soils.Count(), 2);
        
    }

    /// <summary>
    /// Use case: iPad soil app user loads a specific soil from the Apsoil database and views it on iPad
    /// </summary>
    [Test]
    public void iPadAppViewSoilDetails()
    {
        // soil app gets the json for a specific soil
        Service S = new Service();
        string json = S.SoilAsJson("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)");

        // remove the #text so that we can use dynamic below.
        json = json.Replace("#text", "text");

        // user looks at detail on the iPad
        JObject o = JObject.Parse(json);
        Assert.AreEqual(o["soil"]["@name"].ToString(), "Black Vertosol-Orion (Capella No049)");
        Assert.AreEqual(o["soil"]["ASC_Order"]["text"].ToString(), "Vertosol");
        Assert.AreEqual(o["soil"]["Latitude"]["text"].ToString(), "-22.962");
        Assert.AreEqual(o["soil"]["Longitude"]["text"].ToString(), "147.801");
        Assert.AreEqual(o["soil"]["ApsoilNumber"]["text"].ToString(), "49");
        Assert.AreEqual(o["soil"]["ASC_Sub-order"]["text"].ToString(), "Black");
        Assert.AreEqual(o["soil"]["Comments"]["text"].ToString(), "OC and pH estimated");
        Assert.AreEqual(o["soil"]["Country"].ToString(), "Australia");
        Assert.AreEqual(o["soil"]["DataSource"]["text"].ToString(), "Qld Department of Primary Industries and Fisheries (D Lack)");
        //Assert.AreEqual(o["soil"]["LocalName"]["text"].ToString(), "");
        Assert.AreEqual(o["soil"]["LocationAccuracy"]["text"].ToString(), "+/- 20m");
        Assert.AreEqual(o["soil"]["NaturalVegetation"]["text"].ToString(), "Qld Bluegrass, Mountain Coolibah");
        Assert.AreEqual(o["soil"]["NearestTown"]["text"].ToString(), "Capella, Q 4723");
        Assert.AreEqual(o["soil"]["Region"].ToString(), "Central Highlands");
        Assert.AreEqual(o["soil"]["Site"].ToString(), "Capella");
        Assert.AreEqual(o["soil"]["SoilType"]["text"].ToString(), "Clay");
        Assert.AreEqual(o["soil"]["State"].ToString(), "Queensland");

        dynamic p1 = o;

        // Water info

        Assert.AreEqual(p1.soil.Water.Layer.Count, 6);

        Assert.AreEqual(p1.soil.Water.Layer[0].Thickness.text.Value, "150");
        Assert.AreEqual(p1.soil.Water.Layer[1].Thickness.Value, "150");
        Assert.AreEqual(p1.soil.Water.Layer[2].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.Layer[3].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.Layer[4].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.Layer[5].Thickness.Value, "300");

        //Assert.AreEqual(p1.soil.Water.Layer[0].KS.text.Value, "");
        Assert.AreEqual(p1.soil.Water.Layer[1].KS.Value, null);
        Assert.AreEqual(p1.soil.Water.Layer[2].KS.Value, null);
        Assert.AreEqual(p1.soil.Water.Layer[3].KS.Value, null);
        Assert.AreEqual(p1.soil.Water.Layer[4].KS.Value, null);
        Assert.AreEqual(p1.soil.Water.Layer[5].KS.Value, null);

        Assert.AreEqual(p1.soil.Water.Layer[0].BD.text.Value, "1.04");
        Assert.AreEqual(p1.soil.Water.Layer[1].BD.text.Value, "1.22");
        Assert.AreEqual(p1.soil.Water.Layer[2].BD.text.Value, "1.21");
        Assert.AreEqual(p1.soil.Water.Layer[3].BD.text.Value, "1.32");
        Assert.AreEqual(p1.soil.Water.Layer[4].BD.text.Value, "1.42");
        Assert.AreEqual(p1.soil.Water.Layer[5].BD.text.Value, "1.5");

        Assert.AreEqual(p1.soil.Water.Layer[0].Airdry.text.Value, "0.125");
        Assert.AreEqual(p1.soil.Water.Layer[1].Airdry.text.Value, "0.224");
        Assert.AreEqual(p1.soil.Water.Layer[2].Airdry.text.Value, "0.33");
        Assert.AreEqual(p1.soil.Water.Layer[3].Airdry.text.Value, "0.24");
        Assert.AreEqual(p1.soil.Water.Layer[4].Airdry.text.Value, "0.32");
        Assert.AreEqual(p1.soil.Water.Layer[5].Airdry.text.Value, "0.3");

        Assert.AreEqual(p1.soil.Water.Layer[0].LL15.text.Value, "0.25");
        Assert.AreEqual(p1.soil.Water.Layer[1].LL15.text.Value, "0.28");
        Assert.AreEqual(p1.soil.Water.Layer[2].LL15.text.Value, "0.33");
        Assert.AreEqual(p1.soil.Water.Layer[3].LL15.text.Value, "0.24");
        Assert.AreEqual(p1.soil.Water.Layer[4].LL15.text.Value, "0.32");
        Assert.AreEqual(p1.soil.Water.Layer[5].LL15.text.Value, "0.3");

        Assert.AreEqual(p1.soil.Water.Layer[0].DUL.text.Value, "0.48");
        Assert.AreEqual(p1.soil.Water.Layer[1].DUL.text.Value, "0.46");
        Assert.AreEqual(p1.soil.Water.Layer[2].DUL.text.Value, "0.46");
        Assert.AreEqual(p1.soil.Water.Layer[3].DUL.text.Value, "0.35");
        Assert.AreEqual(p1.soil.Water.Layer[4].DUL.text.Value, "0.33");
        Assert.AreEqual(p1.soil.Water.Layer[5].DUL.text.Value, "0.33");

        Assert.AreEqual(p1.soil.Water.Layer[0].SAT.text.Value, "0.58");
        Assert.AreEqual(p1.soil.Water.Layer[1].SAT.text.Value, "0.51");
        Assert.AreEqual(p1.soil.Water.Layer[2].SAT.text.Value, "0.51");
        Assert.AreEqual(p1.soil.Water.Layer[3].SAT.text.Value, "0.47");
        Assert.AreEqual(p1.soil.Water.Layer[4].SAT.text.Value, "0.43");
        Assert.AreEqual(p1.soil.Water.Layer[5].SAT.text.Value, "0.4");

        // Crop info

        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[0].Thickness.text.Value, "150");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[1].Thickness.Value, "150");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[2].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[3].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[4].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[5].Thickness.Value, "300");

        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[0].ll.text.Value, "0.25");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[1].ll.text.Value, "0.28");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[2].ll.text.Value, "0.33");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[3].ll.text.Value, "0.24");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[4].ll.text.Value, "0.32");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[5].ll.text.Value, "0.3");

        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[0].kl.text.Value, "0.06");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[1].kl.Value, "0.06");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[2].kl.Value, "0.06");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[3].kl.Value, "0.04");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[4].kl.Value, "0.04");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[5].kl.Value, "0.02");

        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[0].xf.text.Value, "1");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[1].xf.Value, "1");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[2].xf.Value, "1");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[3].xf.Value, "1");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[4].xf.Value, "1");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[5].xf.Value, "1");

        // Soil Organic Matter info.
        Assert.AreEqual(p1.soil.SoilOrganicMatter.RootCn.ToString(), "40");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.RootWt.ToString(), "1000");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.SoilCn.ToString(), "12");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.EnrACoeff.ToString(), "7.4");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.EnrBCoeff.ToString(), "0.2");

        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[0].Thickness.text.Value, "150");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[1].Thickness.Value, "150");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[2].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[3].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[4].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[5].Thickness.Value, "300");

        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[0].FINERT.text.Value, "0.4");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[1].FINERT.Value, "0.6");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[2].FINERT.Value, "0.8");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[3].FINERT.Value, "1");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[4].FINERT.Value, "1");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[5].FINERT.Value, "1");

        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[0].FBIOM.text.Value, "0.04");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[1].FBIOM.Value, "0.02");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[2].FBIOM.Value, "0.02");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[3].FBIOM.Value, "0.02");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[4].FBIOM.Value, "0.01");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[5].FBIOM.Value, "0.01");

        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[0].OC.text.Value, "0.953846153846154");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[1].OC.text.Value, "0.953846153846154");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[2].OC.text.Value, "0.961538461538461");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[3].OC.text.Value, "0.846153846153846");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[4].OC.text.Value, "0.546153846153846");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[5].OC.text.Value, "0.261538461538462");

        // Analysis

        Assert.AreEqual(p1.soil.Analysis.Layer[0].Thickness.text.Value, "150");
        Assert.AreEqual(p1.soil.Analysis.Layer[1].Thickness.Value, "150");
        Assert.AreEqual(p1.soil.Analysis.Layer[2].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Analysis.Layer[3].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Analysis.Layer[4].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Analysis.Layer[5].Thickness.Value, "300");

        //Assert.AreEqual(p1.soil.Analysis.Layer[0].Rocks.text.Value, "");
        //Assert.AreEqual(p1.soil.Analysis.Layer[1].Rocks.Value, "");
        //Assert.AreEqual(p1.soil.Analysis.Layer[2].Rocks.Value, "");
        //Assert.AreEqual(p1.soil.Analysis.Layer[3].Rocks.Value, "");
        //Assert.AreEqual(p1.soil.Analysis.Layer[4].Rocks.Value, "");
        //Assert.AreEqual(p1.soil.Analysis.Layer[5].Rocks.Value, "");

        Assert.AreEqual(p1.soil.Analysis.Layer[0].Texture.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].Texture.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].Texture.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].Texture.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].Texture.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].Texture.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].MunsellColour.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].MunsellColour.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].MunsellColour.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].MunsellColour.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].MunsellColour.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].MunsellColour.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].EC.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].EC.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].EC.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].EC.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].EC.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].EC.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[1].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[2].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[3].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[4].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[5].PH.text.Value, "8");

        Assert.AreEqual(p1.soil.Analysis.Layer[0].CL.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].CL.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].CL.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].CL.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].CL.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].CL.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].Boron.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].Boron.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].Boron.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].Boron.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].Boron.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].Boron.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].CEC.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].CEC.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].CEC.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].CEC.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].CEC.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].CEC.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].Ca.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].Ca.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].Ca.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].Ca.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].Ca.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].Ca.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].Mg.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].Mg.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].Mg.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].Mg.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].Mg.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].Mg.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].Na.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].Na.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].Na.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].Na.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].Na.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].Na.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].K.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].K.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].K.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].K.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].K.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].K.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].ESP.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].ESP.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].ESP.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].ESP.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].ESP.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].ESP.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].Mn.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].Mn.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].Mn.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].Mn.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].Mn.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].Mn.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].Al.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].Al.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].Al.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].Al.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].Al.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].Al.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].ParticleSizeSand.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].ParticleSizeSand.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].ParticleSizeSand.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].ParticleSizeSand.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].ParticleSizeSand.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].ParticleSizeSand.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].ParticleSizeSilt.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].ParticleSizeSilt.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].ParticleSizeSilt.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].ParticleSizeSilt.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].ParticleSizeSilt.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].ParticleSizeSilt.Value, null);

        Assert.AreEqual(p1.soil.Analysis.Layer[0].ParticleSizeClay.text, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[1].ParticleSizeClay.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[2].ParticleSizeClay.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[3].ParticleSizeClay.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[4].ParticleSizeClay.Value, null);
        Assert.AreEqual(p1.soil.Analysis.Layer[5].ParticleSizeClay.Value, null);
    }

    /// <summary>
    /// Use case: iPad soil app user selects a soil and views the crop PAWC numbers.
    /// </summary>
    [Test]
    public void iPadAppViewCropPAWC()
    {
        Service S = new Service();
        string json = S.SoilAsJson("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)");
        Service.PAWCByCrop[] PAWCs = S.PAWCJson(new Service.PAWCJsonParams {JSonSoil = json});

        Assert.AreEqual(PAWCs.Count(), 1);
        Assert.AreEqual(PAWCs[0].CropName, "wheat");
        Assert.AreEqual(PAWCs[0].PAWC, new double[] { 34.5, 27, 39, 33, 3, 9, }, 0.1);
        Assert.AreEqual(PAWCs[0].PAWCTotal, 145.5);
    }

    /// <summary>
    /// Use case: iPad soil app user selects a soil and views a chart.
    /// </summary>
    [Test]
    public void iPadAppViewSoilChart()
    {
        Service S = new Service();
        string json = S.SoilAsJson("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)");
        XmlDocument doc = JsonConvert.DeserializeXmlNode(json);
        byte[] ChartBytes = S.SoilChartPNGFromXML(doc.OuterXml);
        Assert.AreEqual(ChartBytes.Length, 19041);
    }

    #endregion

}

