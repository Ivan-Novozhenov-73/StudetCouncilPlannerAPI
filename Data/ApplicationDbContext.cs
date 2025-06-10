using Microsoft.EntityFrameworkCore;
using StudetCouncilPlannerAPI.Models.Entities;

namespace StudetCouncilPlannerAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Models.Entities.Task> Tasks { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Note> Notes { get; set; }

        // Связующие таблицы
        public DbSet<EventUser> EventUsers { get; set; }
        public DbSet<TaskUser> TaskUsers { get; set; }
        public DbSet<MeetingUser> MeetingUsers { get; set; }
        public DbSet<EventPartner> EventPartners { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            // EventUser
            modelBuilder.Entity<EventUser>()
                .HasKey(eu => new { eu.UserId, eu.EventId });

            modelBuilder.Entity<EventUser>()
                .HasOne(eu => eu.User)
                .WithMany(u => u.EventUsers)
                .HasForeignKey(eu => eu.UserId);

            modelBuilder.Entity<EventUser>()
                .HasOne(eu => eu.Event)
                .WithMany(e => e.EventUsers)
                .HasForeignKey(eu => eu.EventId);

            // TaskUser
            modelBuilder.Entity<TaskUser>()
                .HasKey(tu => new { tu.UserId, tu.TaskId });

            modelBuilder.Entity<TaskUser>()
                .HasOne(tu => tu.User)
                .WithMany(u => u.TaskUsers)
                .HasForeignKey(tu => tu.UserId);

            modelBuilder.Entity<TaskUser>()
                .HasOne(tu => tu.Task)
                .WithMany(t => t.TaskUsers)
                .HasForeignKey(tu => tu.TaskId);

            // MeetingUser
            modelBuilder.Entity<MeetingUser>()
                .HasKey(mu => new { mu.UserId, mu.MeetingId });

            modelBuilder.Entity<MeetingUser>()
                .HasOne(mu => mu.User)
                .WithMany(u => u.MeetingUsers)
                .HasForeignKey(mu => mu.UserId);

            modelBuilder.Entity<MeetingUser>()
                .HasOne(mu => mu.Meeting)
                .WithMany(m => m.MeetingUsers)
                .HasForeignKey(mu => mu.MeetingId);

            // EventPartner
            modelBuilder.Entity<EventPartner>()
                .HasKey(ep => new { ep.PartnerId, ep.EventId });

            modelBuilder.Entity<EventPartner>()
                .HasOne(ep => ep.Partner)
                .WithMany(p => p.EventPartners)
                .HasForeignKey(ep => ep.PartnerId);

            modelBuilder.Entity<EventPartner>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.EventPartners)
                .HasForeignKey(ep => ep.EventId);

            // Task
            modelBuilder.Entity<Models.Entities.Task>()
                .HasOne(t => t.Event)
                .WithMany(e => e.Tasks)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Entities.Task>()
                .HasOne(t => t.Partner)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.PartnerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Note
            modelBuilder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Event)
                .WithMany(e => e.Notes)
                .HasForeignKey(n => n.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}