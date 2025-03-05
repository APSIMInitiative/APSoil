using Microsoft.EntityFrameworkCore;
using SoilAPI.Data;

namespace UnitTests.Helpers;

public class MockDb : IDbContextFactory<SoilDbContext>
{
    public SoilDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SoilDbContext>()
            .UseInMemoryDatabase($"InMemoryTestDb-{DateTime.Now.ToFileTimeUtc()}")
            .Options;

        return new SoilDbContext(options);
    }
}