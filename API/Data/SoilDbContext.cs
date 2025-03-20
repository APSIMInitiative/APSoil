using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data
{
    public class SoilDbContext : DbContext
    {
        public SoilDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Soil> Soils { get; set; }
    }
}
