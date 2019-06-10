using Microsoft.EntityFrameworkCore;

namespace PapyrusCs.Database
{
    public class PapyrusContext : DbContext
    {
        public PapyrusContext(DbContextOptions<PapyrusContext> builderOptions) : base(builderOptions) {}

        public DbSet<Checksum> Checksums { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Checksum>().HasIndex(x => x.LevelDbKey);
        }
    }
}