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
        public DbSet<Message> Messages { get; set; }

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

            modelBuilder.Entity<Message>().ToTable("Messages");
            modelBuilder.Entity<Message>().HasKey(m => m.MessageId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserEmail)
                .IsUnique();

            modelBuilder.Entity<Assignment>()
                .HasIndex(a => a.PatientId)
                .IsUnique();

            modelBuilder.Entity<Surgery>()
                .HasIndex(s => s.PatientId)
                .IsUnique();

            modelBuilder.Entity<Conversation>()
                .HasIndex(c => c.PatientId)
                .IsUnique();

            modelBuilder.Entity<Administrator>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Nurse>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Surgery>()
                .HasOne(s => s.Patient)
                .WithOne(p => p.Surgery)
                .HasForeignKey<Surgery>(s => s.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Surgery>()
                .HasOne(s => s.SurgeryType)
                .WithMany(st => st.Surgeries)
                .HasForeignKey(s => s.SurgeryTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Nurse)
                .WithMany(n => n.Reports)
                .HasForeignKey(r => r.NurseId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Patient)
                .WithOne(p => p.Assignment)
                .HasForeignKey<Assignment>(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Nurse)
                .WithMany(n => n.Assignments)
                .HasForeignKey(a => a.NurseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Patient)
                .WithOne(p => p.Conversation)
                .HasForeignKey<Conversation>(c => c.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Nurse)
                .WithMany(n => n.Conversations)
                .HasForeignKey(c => c.NurseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}