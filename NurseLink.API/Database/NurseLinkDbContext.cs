using Microsoft.EntityFrameworkCore;
using NurseLink.API.Domain.Entities;

namespace NurseLink.API.Database
{
    public class NurseLinkDbContext : DbContext
    {
        public NurseLinkDbContext(DbContextOptions<NurseLinkDbContext> options)
            : base(options)
        {
        }

        public DbSet<Nurse> Nurses { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SurgeryType> SurgeryTypes { get; set; }
        public DbSet<Surgery> Surgeries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Nurse>().ToTable("Nurses");
            modelBuilder.Entity<Nurse>().HasKey(n => n.NurseId);

            modelBuilder.Entity<Patient>().ToTable("Patients");
            modelBuilder.Entity<Patient>().HasKey(p => p.PatientId);

            modelBuilder.Entity<Administrator>().ToTable("Administrators");
            modelBuilder.Entity<Administrator>().HasKey(a => a.AdminId);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(u => u.UserId);



            // The email has to be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserEmail)
                .IsUnique();

            // 1:1 relation between Administrator and User
            modelBuilder.Entity<Administrator>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:1 relation between Nurse and User
            modelBuilder.Entity<Nurse>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:1 relation betweeb Patient and User
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SurgeryType>().ToTable("SurgeryTypes");
            modelBuilder.Entity<SurgeryType>().HasKey(s => s.SurgeryTypeId);

            modelBuilder.Entity<Surgery>().ToTable("Surgeries");
            modelBuilder.Entity<Surgery>().HasKey(s => s.SurgeryId);

            base.OnModelCreating(modelBuilder);
        }
    }
}