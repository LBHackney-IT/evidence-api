using System;
using System.Linq;
using EvidenceApi.V1.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvidenceApi.V1.Infrastructure
{

    public class EvidenceContext : DbContext
    {
        public EvidenceContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<EvidenceRequestEntity> EvidenceRequests { get; set; }
        public DbSet<ResidentEntity> Residents { get; set; }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IEntity && (e.State == EntityState.Added));

            foreach (var entityEntry in entries)
            {
                ((IEntity)entityEntry.Entity).CreatedAt = DateTime.Now;
                ((IEntity)entityEntry.Entity).Id = Guid.NewGuid();
            }

            return base.SaveChanges();
        }
    }
}
