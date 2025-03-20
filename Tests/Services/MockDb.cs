using Microsoft.EntityFrameworkCore;
using API.Data;
using Microsoft.Data.Sqlite;

namespace Tests.Services;

public class MockDb
{
    public static DbContextOptions CreateOptions<T>() where T : DbContext
    {
        //This creates the SQLite connection string to in-memory database
        var connectionStringBuilder = new SqliteConnectionStringBuilder
            { DataSource = ":memory:" };
        var connectionString = connectionStringBuilder.ToString();

        //This creates a SqliteConnectionwith that string
        var connection = new SqliteConnection(connectionString);

        //The connection MUST be opened here
        connection.Open();

        //Now we have the EF Core commands to create SQLite options
        var builder = new DbContextOptionsBuilder<T>();
        builder.UseSqlite(connection);

        return builder.Options;
    }


}