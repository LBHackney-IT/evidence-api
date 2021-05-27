using System;
using System.Linq;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvidenceApi.V1.Infrastructure
{

    public class EvidenceContext : DbContext
    {
        public EvidenceContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AuditEvent> AuditEvents { get; set; }
        public DbSet<EvidenceRequest> EvidenceRequests { get; set; }
        public DbSet<Resident> Residents { get; set; }
        public DbSet<Communication> Communications { get; set; }
        public DbSet<DocumentSubmission> DocumentSubmissions { get; set; }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IEntity && (e.State == EntityState.Added));

            foreach (var entityEntry in entries)
            {
                var entity = ((IEntity) entityEntry.Entity);
                if (entity.CreatedAt == default) entity.CreatedAt = DateTime.UtcNow;
                if (entity.Id == default) entity.Id = Guid.NewGuid();
            }

            return base.SaveChanges();
        }
    }
}
