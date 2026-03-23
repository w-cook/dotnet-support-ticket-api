using DotnetSupportTicketApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetSupportTicketApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<StatusHistory> StatusHistoryEntries => Set<StatusHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.AuthorUser)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
