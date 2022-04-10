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

        public DbSet<Stats> Stats { get; set; }
        public DbSet<Player> Players { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            const string primaryKeyName = "PrimaryKey";
            modelBuilder.Entity<Stats>().Property<int>(primaryKeyName)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Stats>().HasKey(primaryKeyName);
            
            modelBuilder.Entity<Player>().Property(x => x.Id)
                            .ValueGeneratedOnAdd();
            modelBuilder.Entity<Player>().HasKey(x => x.Id);
        }
    }
}