using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IDocumentTypeGateway
    {
        DocumentType GetDocumentTypeById(string id);

        List<DocumentType> GetAll();
    }
}
