using Microsoft.EntityFrameworkCore;
using RaceDay_api.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RaceDay_api.Database
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
          : base(options)
        {
        }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Results> results { get; set; }
        public DbSet<RegisterUser> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
