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
        public DbSet<Report> Reports { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

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

            modelBuilder.Entity<SurgeryType>().ToTable("SurgeryTypes");
            modelBuilder.Entity<SurgeryType>().HasKey(s => s.SurgeryTypeId);

            modelBuilder.Entity<Surgery>().ToTable("Surgeries");
            modelBuilder.Entity<Surgery>().HasKey(s => s.SurgeryId);

            modelBuilder.Entity<Report>().ToTable("Reports");
            modelBuilder.Entity<Report>().HasKey(r => r.ReportId);

            modelBuilder.Entity<Assignment>().ToTable("Assignments");
            modelBuilder.Entity<Assignment>().HasKey(a => a.AssignmentId);

            modelBuilder.Entity<Conversation>().ToTable("Conversations");
            modelBuilder.Entity<Conversation>().HasKey(c => c.ConversationId);

            // The email has to be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserEmail)
                .IsUnique();

            // One patient can only have one assignment
            modelBuilder.Entity<Assignment>()
                .HasIndex(a => a.PatientId)
                .IsUnique();

            // One patient and one nurse can only have one conversation together
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => new { c.PatientId, c.NurseId })
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

            // 1:1 relation between Patient and User
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many reports for one patient
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many reports for one nurse, nurse optional
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Nurse)
                .WithMany(n => n.Reports)
                .HasForeignKey(r => r.NurseId)
                .OnDelete(DeleteBehavior.SetNull);

            // One patient can only have one assignment
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Patient)
                .WithOne(p => p.Assignment)
                .HasForeignKey<Assignment>(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // One nurse can have many assignments
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Nurse)
                .WithMany(n => n.Assignments)
                .HasForeignKey(a => a.NurseId)
                .OnDelete(DeleteBehavior.Cascade);

            // One patient can have one conversation
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Patient)
                .WithOne(p => p.Conversation)
                .HasForeignKey<Conversation>(c => c.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // One nurse can have many conversations
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Nurse)
                .WithMany(n => n.Conversations)
                .HasForeignKey(c => c.NurseId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}