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
        public DbSet<CommunicationEntity> Communications { get; set; }
        public DbSet<DocumentSubmissionEntity> DocumentSubmissions { get; set; }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IEntity && (e.State == EntityState.Added));

            foreach (var entityEntry in entries)
            {
                var entity = ((IEntity) entityEntry.Entity);
                entity.CreatedAt = DateTime.Now;
                entity.Id = Guid.NewGuid();
            }

            return base.SaveChanges();
        }
    }
}
