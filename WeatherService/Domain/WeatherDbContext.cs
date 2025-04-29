using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WeatherService.Domain.Entities;

namespace WeatherService.Domain
{
    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options) { }

        public DbSet<Location> Locations { get; set; }
    }
}
