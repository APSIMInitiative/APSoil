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

    public void UpgradeSoils()
    {
        //Service S = new Service();
        //S.ConvertOldSoilsToNewSoils();
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
    /// Usecase: Yield Prophet needs to create a <sample> 1 node from some data.
    /// </summary>
    [Test]
    public void YPGetSoilSample1XML()
    {
        Service S = new Service();
        // Make sure we can create a soil sample 1.
        string[] Depths = new string[] { "0-10", "10-40", "40-70", "70-100" };
        double[] SW = new double[] { 0.139, 0.215, 0.179, 0.213 };  // not % but mm/mm - divide percent by 100
        double[] NO3 = new double[] { 9, 19, 9, 2 };
        double[] NH4 = new double[] { 6, 6, 6, 4 };
        string SWUnits = "grav. mm/mm";   // This is volumetic - alternative units: "grav. mm/mm";
        string SoilSample1XML = S.CreateSoilSample1XML(new DateTime(2011, 12, 25), Depths, SWUnits, SW, NO3, NH4);
        // Make sure we can parse the XML
        XmlDocument Doc = new XmlDocument();
        Doc.LoadXml(SoilSample1XML);
    }

    /// <summary>
    /// Usecase: Yield Prophet needs to create a <sample> 2 node from some data.
    /// </summary>
    [Test]
    public void YPGetSoilSample2XML()
    {
        Service S = new Service();
        // Make sure we can create a soil sample 1.
        string[] Depths = new string[] { "0-10", "10-40" };
        double[] OC = new double[] { 1.0, 0.8 };  
        double[] EC = new double[] { 9, 19 };
        double[] PH = new double[] { 6, 6 };
        double[] Cl = new double[] { 6, 6 };
        string SoilSample2XML = S.CreateSoilSample2XML(new DateTime(2011, 12, 25), Depths, OC, EC, PH, Cl);
        // Make sure we can parse the XML
        XmlDocument Doc = new XmlDocument();
        Doc.LoadXml(SoilSample2XML);
    }

    /// <summary>
    /// Usecase: Yield Prophet user selects a soil and displays a chart.
    /// </summary>
    [Test]
    public void YPViewSoilChart()
    {
        Service S = new Service();
        byte[] ChartBytes = S.SoilChartPNG("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)");
        Assert.AreEqual(ChartBytes.Length, 22314);
    }

    /// <summary>
    /// Usecase: Yield Prophet user selects a soil and a sample and displays a chart.
    /// </summary>
    [Test]
    public void YPViewSoilChartWithSample()
    {
        Service S = new Service();

        string[] Depths = new string[] { "0-10", "10-40", "40-70", "70-100" };
        double[] SW = new double[] { 0.139, 0.215, 0.179, 0.213 };  // not % but mm/mm - divide percent by 100
        double[] NO3 = new double[] { 9, 19, 9, 2 };
        double[] NH4 = new double[] { 6, 6, 6, 4 };
        string SWUnits = "grav. mm/mm";   // This is volumetic - alternative units: "grav. mm/mm";
        string SoilSample1XML = S.CreateSoilSample1XML(new DateTime(2011, 12, 25), Depths, SWUnits, SW, NO3, NH4);

        byte[] ChartBytes = S.SoilChartWithSamplePNG("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)",
                                                     SoilSample1XML);
        Assert.AreEqual(ChartBytes.Length, 32533);
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

        string[] Depths = new string[] { "0-10", "10-40", "40-70", "70-100" };
        double[] SW = new double[] { 0.139, 0.215, 0.179, 0.213 };  // not % but mm/mm - divide percent by 100
        double[] NO3 = new double[] { 9, 19, 9, 2 };
        double[] NH4 = new double[] { 6, 6, 6, 4 };
        string SWUnits = "grav. mm/mm";   // This is volumetic - alternative units: "grav. mm/mm";
        string SoilSample1XML = S.CreateSoilSample1XML(new DateTime(2011, 12, 25), Depths, SWUnits, SW, NO3, NH4);


        double PAW = S.PAW("Soils/Australia/Queensland/Central Highlands/Black Vertosol-Orion (Capella No049)", 
                           SoilSample1XML,
                           "Wheat");
        Assert.AreEqual(PAW, 6.48, 0.01);
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
        Assert.AreEqual(MatchingSoils.Count(), 14);
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
        Assert.AreEqual(Soils[0].Name, "Soils/Australia/Queensland/Darling Downs and Granite Belt/Red Chromosol (Billa Billa No066)");
        Assert.AreEqual(Soils[0].Latitude, -28.162);
        Assert.AreEqual(Soils[0].Longitude, 150.201);
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


        dynamic p1 = o;

        Assert.AreEqual(p1.soil.Water.Layer.Count, 6);

        Assert.AreEqual(p1.soil.Water.Layer[0].Thickness.text.Value, "150");
        Assert.AreEqual(p1.soil.Water.Layer[1].Thickness.Value, "150");
        Assert.AreEqual(p1.soil.Water.Layer[2].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.Layer[3].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.Layer[4].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.Layer[5].Thickness.Value, "300");

        Assert.AreEqual(p1.soil.Water.Layer[0].LL15.text.Value, "0.25");
        Assert.AreEqual(p1.soil.Water.Layer[1].LL15.text.Value, "0.28");
        Assert.AreEqual(p1.soil.Water.Layer[2].LL15.text.Value, "0.33");
        Assert.AreEqual(p1.soil.Water.Layer[3].LL15.text.Value, "0.24");
        Assert.AreEqual(p1.soil.Water.Layer[4].LL15.text.Value, "0.32");
        Assert.AreEqual(p1.soil.Water.Layer[5].LL15.text.Value, "0.3");

        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[0].Thickness.text.Value, "150");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[1].Thickness.Value, "150");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[2].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[3].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[4].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Water.SoilCrop.Layer[5].Thickness.Value, "300");

        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[0].Thickness.text.Value, "150");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[1].Thickness.Value, "150");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[2].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[3].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[4].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[5].Thickness.Value, "300");

        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[0].OC.text.Value, "0.953846153846154");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[1].OC.text.Value, "0.953846153846154");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[2].OC.text.Value, "0.961538461538461");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[3].OC.text.Value, "0.846153846153846");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[4].OC.text.Value, "0.546153846153846");
        Assert.AreEqual(p1.soil.SoilOrganicMatter.Layer[5].OC.text.Value, "0.261538461538462");

        Assert.AreEqual(p1.soil.Analysis.Layer[0].Thickness.text.Value, "150");
        Assert.AreEqual(p1.soil.Analysis.Layer[1].Thickness.Value, "150");
        Assert.AreEqual(p1.soil.Analysis.Layer[2].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Analysis.Layer[3].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Analysis.Layer[4].Thickness.Value, "300");
        Assert.AreEqual(p1.soil.Analysis.Layer[5].Thickness.Value, "300");

        Assert.AreEqual(p1.soil.Analysis.Layer[0].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[1].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[2].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[3].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[4].PH.text.Value, "8");
        Assert.AreEqual(p1.soil.Analysis.Layer[5].PH.text.Value, "8");
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
        Assert.AreEqual(ChartBytes.Length, 22314);
    }

    #endregion

}

