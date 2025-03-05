using Microsoft.EntityFrameworkCore;
using SoilAPI.Models;

namespace SoilAPI.Data
{
    public class SoilDbContext : DbContext
    {
        public SoilDbContext(DbContextOptions<SoilDbContext> options) : base(options) { }

        public DbSet<Soil> Soils { get; set; }
    }
}
