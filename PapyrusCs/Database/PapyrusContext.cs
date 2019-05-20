﻿using Microsoft.EntityFrameworkCore;

namespace PapyrusCs.Database
{
    public class PapyrusContext : DbContext
    {
        public PapyrusContext(DbContextOptions<PapyrusContext> builderOptions) : base(builderOptions) {}

        public DbSet<Checksum> Checksums { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Checksum>().HasIndex(x => x.LevelDbKey).IsUnique(true);
        }
    }
}