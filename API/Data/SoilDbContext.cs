using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data
{
    public class SoilDbContext : DbContext
    {
        public SoilDbContext(DbContextOptions<SoilDbContext> options) : base(options) { }

        public DbSet<Soil> Soils { get; set; }
    }
}
