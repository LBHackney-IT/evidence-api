using System.Collections.Generic;
using DocumentsApi.V1.Domain;

namespace DocumentsApi.V1.Gateways
{
    public interface IExampleGateway
    {
        Entity GetEntityById(int id);

        List<Entity> GetAll();
    }
}
