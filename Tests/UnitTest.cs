using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
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
        var result = API.Services.Soil.Add(context, [ soil ]);

        Assert.That(result, Is.InstanceOf<Ok>());
    }

    [Test]
    public async Task AddModifiedSoil_ShouldOverwriteExistingSoil()
    {
        await using var context = new Services.MockDb().CreateDbContext();
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");

        // Add soil
        var result = API.Services.Soil.Add(context, [ soil ]);
        Assert.That(result, Is.InstanceOf<Ok>());

        // Change a field
        soil.Region = "New region";

        // Add soil with new region.
        result = API.Services.Soil.Add(context, [ soil ]);
        Assert.That(result, Is.InstanceOf<Ok>());

        // check the soil has been updated
        var updatedSoil = context.Soils
                                 .Where(s => s.Name == "Clay (Kerikeri No1353)")
                                 .FirstOrDefault();
        Assert.That(updatedSoil.Region, Is.EqualTo("New region"));
    }

    [Test]
    public async Task AddOldSoilFolder_ShouldHaveFullNames()
    {
        await using var context = new Services.MockDb().CreateDbContext();
        var folder = ResourceFile.FromResourceXML<API.Models.Folder>("Tests.testfolder.xml");
        var result = API.Services.Soil.AddOld(context, folder);

        Assert.That(result, Is.InstanceOf<Ok>());

        // Ensure all soils have full name set.
        var soils = context.Soils.ToList();
        foreach (var soil in soils)
            Assert.That(soil.FullName, Is.Not.Null);
    }

    [Test]
    public async Task GetWithNoArguments_ShouldReturnAllSoils()
    {
        await using var context = new Services.MockDb().CreateDbContext();

        // Add 2 soils.
        var soil1 = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil1.xml");
        var soil2 = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.testsoil2.xml");
        API.Services.Soil.Add(context, [ soil1, soil2 ]);

        // Get soils.
        var soils = API.Services.Soil.Get(context);

        Assert.That(soils.Length, Is.EqualTo(2));
    }

}