using Microsoft.EntityFrameworkCore;
using API.Data;

namespace Tests.Services;

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