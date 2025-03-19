using Microsoft.EntityFrameworkCore;
using API.Data;
using EntityFrameworkCore.Testing.Moq;

namespace Tests.Services;

public class MockDb : IDbContextFactory<SoilDbContext>
{

    public SoilDbContext CreateDbContext()
    {
        return CreateDbContext(true);
    }

    public SoilDbContext CreateDbContext(bool deleteExisting)
    {
        // remove existing database
        if (File.Exists("unittest.db") && deleteExisting)
            File.Delete("unittest.db");

        var options = new DbContextOptionsBuilder<SoilDbContext>()
            .UseSqlite("Data Source=unittest.db")
            .Options;

        var dbContext = new SoilDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }


}