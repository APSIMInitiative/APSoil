using System.Drawing;
using API.Data;
using API.Services;
using NUnit.Framework;
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
        API.Services.Soil.Add(context, [ soil ]);

        // Change a field
        soil.Region = "New region";

        // Add soil with new region.
        API.Services.Soil.Add(context, [ soil ]);

        // check the soil has been updated
        var updatedSoil = context.Soils
                                 .Where(s => s.Name == "Clay (Kerikeri No1353)")
                                 .FirstOrDefault();
        Assert.That(updatedSoil.Region, Is.EqualTo("New region"));
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

        var soils = API.Services.Soil.Search(context, latitude:-28, longitude: 150, radius: 100)
                                     .ToSoils();

        Assert.That(soils.Length, Is.EqualTo(1));
        Assert.That(soils[0].Name, Is.EqualTo("Red Chromosol (Billa Billa No066)"));

        // Extend the radius to cover the second soil (New Zealand)
        soils = API.Services.Soil.Search(context, latitude:-28, longitude: 150, radius: 2500)
                                 .ToSoils();
        Assert.That(soils.Length, Is.EqualTo(2));
        Assert.That(soils[0].Name, Is.EqualTo("Red Chromosol (Billa Billa No066)"));
        Assert.That(soils[1].Name, Is.EqualTo("Clay (Kerikeri No1353)"));
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

        Assert.That(xml, Is.EqualTo(ResourceFile.Get("Tests.testsoil12.xml")));
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
    public void Graph_ShouldReturnGraph()
    {
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");
        using var ms = new MemoryStream(soil.ToGraphPng());
        using (var fileStream = new FileStream("Tests.testgraph.png", FileMode.Create, FileAccess.Write))
            ms.CopyTo(fileStream);
        Assert.That(File.Exists("Tests.testgraph.png"), Is.True);
    }
}