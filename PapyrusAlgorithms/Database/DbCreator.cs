using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PapyrusAlgorithms.Database
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

        public PapyrusContext CreateDbContext(string filename, bool tracking)
        {
            var builder = new DbContextOptionsBuilder<PapyrusContext>();
            builder.UseSqlite($"Filename=\"{filename}\"");
            if (!tracking)
            {
                builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }

            return new PapyrusContext(builder.Options);
        }

    }
}