using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways
{
    public interface IExampleGateway
    {
        Entity GetEntityById(int id);

        List<Entity> GetAll();
    }
}
