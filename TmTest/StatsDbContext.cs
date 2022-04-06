using Microsoft.EntityFrameworkCore;

namespace TmTest
{
    public class StatsDbContext : DbContext
    {
        private const string ConnectionString = "Data Source=stats.db";

        public StatsDbContext()
        {
            Database?.EnsureCreated();
        }

        DbSet<Stats> Stats { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stats>().Property<int>("PrimaryKey")
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Stats>().HasKey("PrimaryKey");
        }
    }
}