using Microsoft.EntityFrameworkCore;
using RaceDay_api.Models;
using ResultsModel = RaceDay_api.Models.Results;

namespace RaceDay_api.Database
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(
            DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<Enrolment> Enrolments { get; set; }

        public DbSet<ResultsModel> results { get; set; }

        public DbSet<RegisterUser> Users { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enrolment
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany()
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrolment
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrolment 
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Result)
                .WithOne(r => r.Enrolment)
                .HasForeignKey<ResultsModel>(
                    r => r.EnrolmentID);

            // Event 
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany()
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Categories)
                .WithOne()
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}