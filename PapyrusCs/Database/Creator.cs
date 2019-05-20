using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PapyrusCs.Database
{
    public class Creator : IDesignTimeDbContextFactory<PapyrusContext>
    {
        public PapyrusContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<PapyrusContext>();
            builder.UseSqlite("Filename=\"UnicornClicker2.db\"");
            return new PapyrusContext(builder.Options);
        }

        public PapyrusContext CreateDbContext(string filename)
        {
            var builder = new DbContextOptionsBuilder<PapyrusContext>();
            builder.UseSqlite($"Filename=\"{filename}\"");
            return new PapyrusContext(builder.Options);
        }

    }
}