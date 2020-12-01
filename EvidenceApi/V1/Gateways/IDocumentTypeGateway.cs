using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways
{
    public interface IDocumentTypeGateway
    {
        DocumentType GetDocumentTypeById(string id);

        List<DocumentType> GetAll();
    }
}
