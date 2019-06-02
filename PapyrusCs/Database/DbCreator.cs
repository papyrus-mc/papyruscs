using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PapyrusCs.Database
{
    public class DbCreator : IDesignTimeDbContextFactory<PapyrusContext>
    {
        public PapyrusContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<PapyrusContext>();
            builder.UseSqlite("Filename=\"UnicornClicker2.db\"");
            builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new PapyrusContext(builder.Options);
        }

        public PapyrusContext CreateDbContext(string filename)
        {
            var builder = new DbContextOptionsBuilder<PapyrusContext>();
            builder.UseSqlite($"Filename=\"{filename}\"");
            builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new PapyrusContext(builder.Options);
        }

    }
}