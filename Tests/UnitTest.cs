using System.Data;
using System.Xml.Serialization;
using API.Data;
using API.Services;
using APSIM.Graphs;
using NUnit.Framework;
using SharpKml.Engine;
using Tests.Services;

namespace Tests;

[TestFixture]
public class UploadEndpointTests
{

    [Test]
    public void AddSoil_ShouldReturnOk()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");
        API.Services.Soil.Add(context, [ soil ]);

        Assert.That(context.Soils.Count(), Is.EqualTo(1));
    }

    [Test]
    public void AddModifiedSoil_ShouldOverwriteExistingSoil()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");

        // Add soil
        API.Services.Soil.Add(context, [soil]);

        // Change a field
        soil.Region = "New region";

        // Add soil with new region.
        API.Services.Soil.Add(context, [soil]);

        // check the soil has been updated
        var updatedSoil = context.Soils
                                 .Where(s => s.Name == "Clay (Kerikeri No1353)")
                                 .FirstOrDefault();
        Assert.That(updatedSoil.Region, Is.EqualTo("New region"));
    }

    /// <summary>
    /// The Farm4Prophet soils have trailing spaces in their names. Need to make sure
    /// they can be retrieved.
    /// https://github.com/APSIMInitiative/APSoil/issues/51
    /// </summary>
    [Test]
    public void GetUsingFullName_ShouldIgnoreTrailingSpaces()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        var folder = ResourceFile.Get("Tests.testsoil1-trailing-spaces.xml").ToSoils();

        API.Services.Soil.Add(context, folder);

        var soils = API.Services.Soil.Search(context, fullName: "Soils/Tests/Clay (Kerikeri No1353)")
                                     .ToSoils();
        Assert.That(soils.Length, Is.EqualTo(1));
        Assert.That(soils[0].Name.Trim(), Is.EqualTo("Clay (Kerikeri No1353)"));
    }

    [Test]
    public void GetWithLatLong_ShouldReturnSoilsClosestToPoint()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        API.Services.Soil.Add(context, [
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml")
         ]);

        var soils = API.Services.Soil.Search(context, numToReturn: 1,
                                             latitude:-28, longitude: 150)
                                     .ToSoils();
        Assert.That(soils.Length, Is.EqualTo(1));
        Assert.That(soils[0].Name, Is.EqualTo("Red Chromosol (Billa Billa No066)"));
    }

    [Test]
    public void GetWithLatLongRadius_ShouldReturnSoilsWithinTheRadius()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        API.Services.Soil.Add(context, [
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml")
         ]);

        var soils = API.Services.Soil.Search(context, latitude: -28, longitude: 150, radius: 100)
                                     .ToSoils();

        Assert.That(soils.Length, Is.EqualTo(1));
        Assert.That(soils[0].Name, Is.EqualTo("Red Chromosol (Billa Billa No066)"));

        // Extend the radius to cover the second soil (New Zealand)
        soils = API.Services.Soil.Search(context, latitude: -28, longitude: 150, radius: 2500)
                                 .ToSoils();
        Assert.That(soils.Length, Is.EqualTo(2));
        Assert.That(soils[0].Name, Is.EqualTo("Red Chromosol (Billa Billa No066)"));
        Assert.That(soils[1].Name, Is.EqualTo("Clay (Kerikeri No1353)"));
        Assert.That(soils[0].SoilOrganicMatter, Is.Not.Null);
        Assert.That(soils[0].Analysis, Is.Not.Null);
        Assert.That(soils[0].SoilWater, Is.Not.Null);
        Assert.That(soils[0].Water, Is.Not.Null);
    }

    [Test]
    public void GetWithMultiplePAWC_ShouldReturnSoilWithClosestPAWC()
    {
        // SOIL1			                        SOIL2
        // thickness	CLL	    DUL	    PAWC        thickness	CLL	    DUL	  PAWC
        // 150	        0.282	0.502	33          100	       0.12	    0.34	22
        // 120	        0.295	0.437	17.04       100	       0.13	    0.38	25
        // 180	        0.313	0.439	22.68       300	       0.16	    0.37	63
        // 270	        0.304	0.435	35.37       300	       0.22	    0.37	45
        // 380	        0.286	0.426	53.2        300	       0.3	    0.36	18
        // 400	        0.295	0.429	53.6        300	       0.32	    0.36	12

        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        API.Services.Soil.Add(context, [
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml")
         ]);

        var soils = API.Services.Soil.Search(context, cropName: "wheat", numToReturn: 1,
                                             thickness: [ 200, 300, 300 ], pawc: [ 20, 60, 45 ])
                                     .ToSoils();
        Assert.That(soils.Length, Is.EqualTo(1));
        Assert.That(soils[0].Name, Is.EqualTo("Red Chromosol (Billa Billa No066)"));
    }

    [Test]
    public void GetWithSinglePAWC_ShouldReturnSoilWithClosestPAWC()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        API.Services.Soil.Add(context, [
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml")
         ]);

        var soils = API.Services.Soil.Search(context, cropName: "wheat", numToReturn: 1,
                                             thickness: [ 800 ], pawc: [ 120 ])
                                     .ToSoils();
        Assert.That(soils.Length, Is.EqualTo(1));
        Assert.That(soils[0].Name, Is.EqualTo("Red Chromosol (Billa Billa No066)"));
    }

    [Test]
    public void GetWithCLL_ShouldReturnSoilWithClosestCLL()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        API.Services.Soil.Add(context, [
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml")
         ]);

        var soils = API.Services.Soil.Search(context, cropName: "wheat", numToReturn: 1,
                                             thickness: [ 200, 300, 300 ], cll: [ 0.12, 0.18, 0.22 ])
                                     .ToSoils();
        Assert.That(soils.Length, Is.EqualTo(1));
        Assert.That(soils[0].Name, Is.EqualTo("Clay (Kerikeri No1353)"));
    }

    [Test]
    public void GetWithRecordNumber_ShouldReturnSoil()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        API.Services.Soil.Add(context, [
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml")
         ]);

        var soils = API.Services.Soil.Search(context, recordNumber: 1)
                                     .ToSoils();
        Assert.That(soils.Length, Is.EqualTo(1));
        Assert.That(soils[0].Name, Is.EqualTo("Red Chromosol (Billa Billa No066)"));
    }

    [Test]
    public void SoilsToXML_ShouldReturnValidXML()
    {
        // Get 2 soils.
        API.Models.Soil[] soils = [ ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
                                    ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml") ];

        // Get soils.
        var xml = soils.ToFolder().ToXML();

        XmlSerializer serializer = new(typeof(API.Models.Folder));
        var folder = serializer.Deserialize(new StringReader(xml)) as API.Models.Folder;
        Assert.That(folder.Soils.Count, Is.EqualTo(2));
    }

    [Test]
    public void XMLToSoils_ShouldReturnValidSoils()
    {
        var soils = ResourceFile.Get("Tests.testfolder.xml").ToSoils();
        Assert.That(soils.Length, Is.EqualTo(2));
    }

    [Test]
    public void ToInfo_ShouldReturnInfoAboutASoil()
    {
        // Get 2 soils.
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");

        // Get info.
        var info = soil.ToInfo();

        Assert.That(info.Name, Is.EqualTo("Clay (Kerikeri No1353)"));
        Assert.That(info.SoilType, Is.EqualTo("Clay"));
        Assert.That(info.Latitude, Is.EqualTo(-35.258));
        Assert.That(info.Longitude, Is.EqualTo(173.937));
        Assert.That(info.Thickness, Is.EqualTo(new double[] { 150, 120, 180, 270, 380, 400 }));
        Assert.That(info.Texture, Is.EqualTo(new string[] { "Clay", "Heavy clay", "Heavy clay", "Heavy clay", "Heavy clay", "Silt clay" }));
        Assert.That(info.Crops.Count, Is.EqualTo(2));
        Assert.That(info.Crops[0].Name, Is.EqualTo("wheat"));
        Assert.That(info.Crops[0].LL, Is.EqualTo(new double[] { 0.282, 0.295, 0.313, 0.304, 0.286, 0.295 }));
        Assert.That(info.Crops[1].Name, Is.EqualTo("perennialgrass"));
        Assert.That(info.Crops[1].LL, Is.EqualTo(new double[] { 0.282, 0.295, 0.313, 0.304, 0.286, 0.295 }));
    }

    [Test]
    public void PAWC_ShouldReturnCorrectValue()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        API.Services.Soil.Add(context, [
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
         ]);

        var pawc = API.Services.Soil.PAWC(context, "Clay (Kerikeri No1353)", cropName: "wheat");
        Assert.That(pawc, Is.EqualTo(214.89).Within(0.000001));
    }

    [Test]
    public void PAWC_ShouldReturnCorrectValueFromSoilWithTrailingSpacesInName()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        var folder = ResourceFile.Get("Tests.testsoil1-trailing-spaces.xml").ToSoils();
        API.Services.Soil.Add(context, folder);

        var pawc = API.Services.Soil.PAWC(context, "Soils/Tests/Clay (Kerikeri No1353)", cropName: "wheat");
        Assert.That(pawc, Is.EqualTo(214.89).Within(0.000001));
    }

    [Test]
    public void PAW_ShouldReturnCorrectValue()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        API.Services.Soil.Add(context, [
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
         ]);

        var paw = API.Services.Soil.PAW(context, "Clay (Kerikeri No1353)", cropName: "wheat",
                                        thickness: [ 150, 500 ],
                                        sw: [ 0.4, 0.3 ], swIsGrav: false);
        Assert.That(paw, Is.EqualTo(21.72).Within(0.000001));
    }


    [Test]
    public void PAW_ShouldReturnCorrectValueFromSoilWithTrailingSpacesInName()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        var folder = ResourceFile.Get("Tests.testsoil1-trailing-spaces.xml").ToSoils();
        API.Services.Soil.Add(context, folder);

        var paw = API.Services.Soil.PAW(context, "Soils/Tests/Clay (Kerikeri No1353)", cropName: "wheat",
                                        thickness: [ 150, 500 ],
                                        sw: [ 0.4, 0.3 ], swIsGrav: false);
        Assert.That(paw, Is.EqualTo(21.72).Within(0.000001));
    }

    [Test]
    public void PAWFromGravimetric_ShouldReturnCorrectValue()
    {
        var options = MockDb.CreateOptions<SoilDbContext>();
        using var context = new SoilDbContext(options);
        API.Services.Soil.Add(context, [
            ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
         ]);

        var paw = API.Services.Soil.PAW(context, "Clay (Kerikeri No1353)", cropName: "wheat",
                                        thickness: [ 150, 500 ],
                                        sw: [ 0.4, 0.3 ], swIsGrav: true);
        Assert.That(paw, Is.EqualTo(24.676).Within(0.000001));
    }

    [Test]
    public void KMZ_ShouldReturnValidKMZ()
    {
        var kml = ResourceFile.Get("Tests.testfolder.xml").ToSoils().ToFolder().ToKMZ();
        var s = new MemoryStream(kml);

        // Now open the file we saved and list the contents
        using (KmzFile kmz = KmzFile.Open(s))
        {
            Assert.That(kmz.Files.Count, Is.EqualTo(2));
            Assert.That(kmz.Files.First(), Is.EqualTo("soils.kml"));
            Assert.That(kmz.Files.Last(), Is.EqualTo("shovel.png"));
            var kmlFile = kmz.GetDefaultKmlFile();

            var feature = (kmlFile.Root as SharpKml.Dom.Kml).Feature as SharpKml.Dom.Folder;
            Assert.That(feature.Name, Is.EqualTo("Soils"));
            Assert.That(feature.Features.Count, Is.EqualTo(2));
        }
    }

    [Test]
    public void ToRecursiveFolder_ShouldReturnValidFolder()
    {
        var soils = ResourceFile.Get("Tests.testfolder.xml").ToSoils();
        soils[0].FullName = "Soils/A1/A2/Red Chromosol (Billa Billa No066)";
        soils[1].FullName = "Soils/B1/B2/Grey Vertosol-Mt Carmel (Billa Billa No036)";
        var folder = soils.ToRecursiveFolder();

        Assert.That(folder.Name, Is.EqualTo("Soils"));
        Assert.That(folder.Folders[0].Name, Is.EqualTo("A1"));
        Assert.That(folder.Folders[0].Folders[0].Name, Is.EqualTo("A2"));
        Assert.That(folder.Folders[0].Folders[0].Soils[0].Name, Is.EqualTo("Red Chromosol (Billa Billa No066)"));

        Assert.That(folder.Folders[1].Name, Is.EqualTo("B1"));
        Assert.That(folder.Folders[1].Folders[0].Name, Is.EqualTo("B2"));
        Assert.That(folder.Folders[1].Folders[0].Soils[0].Name, Is.EqualTo("Grey Vertosol-Mt Carmel (Billa Billa No036)"));
    }

    [Test]
    public void ToAPSIMSoil_ShouldPopulateAllMembers()
    {
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");

        var apsimSoilXml = soil.ToAPSIMSoil().ToXML();
        Assert.That(apsimSoilXml, Is.EqualTo(ResourceFile.Get("Tests.testsoil1.apsim.xml")));
    }

    [Test]
    public void ToGraph_ShouldWorkWithoutSWSpecified()
    {
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");

        var graph = soil.ToGraph();
        Assert.That(graph, Is.Not.Null);

        // should be 5 graph series:
        //  shaded bucket, ll, airdry, dul, sat
        Assert.That(graph.Series.Count, Is.EqualTo(5));
    }

    [Test]
    public void ToDataTable_ShouldWork()
    {
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");

        var table = soil.ToDataTable();
        Assert.That(table, Is.Not.Null);
        Assert.That(table.Columns.Count, Is.EqualTo(251));
        Assert.That(table.Rows.Count, Is.EqualTo(6));

        string[] someExpectedNames = [ "Name", "RecordNo", "APSoilNumber", "Latitude", "Longitude",
                                       "BD (g/cc)", "Rocks (%)", "SAT (mm/mm)", "DUL (mm/mm)",
                                       "LL15 (mm/mm)", "Airdry (mm/mm)" ];

        foreach (var name in someExpectedNames)
            Assert.That(table.Columns.Contains(name));
    }

    [Test]
    public void ToCSV_ShouldWork()
    {
        DataTable table = new DataTable()
        {
            Columns =
            {
                { "Col1", typeof(int) },
                { "Col2", typeof(double) }
            },
            Rows =
            {
                { 1, 2 },
                { 3, 4 }
            }
        };

        string csv = table.ToCSV();
        string expectedCsv = "Col1,Col2" + Environment.NewLine +
                             "1,2.000" + Environment.NewLine +
                             "3,4.000" + Environment.NewLine;

        Assert.That(csv, Is.EqualTo(expectedCsv));
    }
}