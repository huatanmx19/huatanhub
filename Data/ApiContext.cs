using Microsoft.EntityFrameworkCore;
using HuatanHub.Entities;

namespace HuatanHub.Data
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {
        }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Tramo> Tramos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Location>().ToTable("Locations");
            builder.Entity<Asistencia>().ToTable("Asistencia");
            builder.Entity<Empleado>().ToTable("Empleado");
            builder.Entity<Tramo>().ToTable("Tramo");
        }

    }
}
