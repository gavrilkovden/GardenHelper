using DataInputService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DataInputService.Domain
{
    public class PlantDbContext : DbContext
    {
        public PlantDbContext(DbContextOptions<PlantDbContext> options)
            : base(options) { }

        public DbSet<Plant> Plants { get; set; }
    }
}
