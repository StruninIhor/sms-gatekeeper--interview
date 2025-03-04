using Microsoft.EntityFrameworkCore;
using SmsGatekeeper.Domain.Models;

namespace SmsGatekeeper.Domain
{
    public class SmsContext : DbContext
    {
        public DbSet<HistoryRecord> HistoryRecords { get; set; }

        public SmsContext(DbContextOptions<SmsContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoryRecord>().HasIndex(x => x.AccountId);
            modelBuilder.Entity<HistoryRecord>().HasIndex(x => x.PhoneNumber);
            base.OnModelCreating(modelBuilder);
        }
    }
}

