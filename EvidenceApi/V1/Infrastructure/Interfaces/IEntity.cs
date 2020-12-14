using System;

namespace EvidenceApi.V1.Infrastructure.Interfaces
{
    public interface IEntity
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
