using System.Linq;
using System.Text.Json;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class UploadEndpointTests
{

    [Test]
    public async Task AddSoil_ShouldReturnOk()
    {
        await using var context = new Services.MockDb().CreateDbContext();
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");
        API.Services.Soil.Add(context, [ soil ]);

        Assert.That(context.Soils.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddModifiedSoil_ShouldOverwriteExistingSoil()
    {
        await using var context = new Services.MockDb().CreateDbContext();
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
    public async Task GetWithNoArguments_ShouldReturnAllSoilsWithAllRelatedData()
    {
        await using (var context = new Services.MockDb().CreateDbContext())
        {

            // Add 2 soils.
            var soil1 = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");
            var soil2 = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml");
            API.Services.Soil.Add(context, [ soil1, soil2 ]);
        }

        // Create a new DBcontext to ensure the data was saved and can be reloaded correctly.
        // This mimics a call to the the API to add data and another call to retrieve data.
        await using var context2 = new Services.MockDb().CreateDbContext(deleteExisting: false);

        // Get soils.
        var soils = API.Services.Soil.Get(context2);

        Assert.That(soils.Length, Is.EqualTo(2));
        Assert.That(soils[0].Analysis, Is.Not.Null);
        Assert.That(soils[0].SoilOrganicMatter, Is.Not.Null);
        Assert.That(soils[0].Water, Is.Not.Null);
        Assert.That(soils[0].SoilWater, Is.Not.Null);
        Assert.That(soils[0].Water.SoilCrops.Count, Is.EqualTo(2));
    }


    [Test]
    public void SoilsToXML_ShouldReturnValidXML()
    {
        // Get 2 soils.
        API.Models.Soil[] soils = [ ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml"),
                                    ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml") ];

        // Get soils.
        var xml = soils.ToXML();

        Assert.That(xml, Is.EqualTo(ResourceFile.Get("Tests.testsoil12.xml")));
    }

    [Test]
    public void XMLToSoils_ShouldReturnValidSoils()
    {
        var soils = ResourceFile.Get("Tests.testfolder.xml").ToSoils();
        Assert.That(soils.Length, Is.EqualTo(2));
    }
}