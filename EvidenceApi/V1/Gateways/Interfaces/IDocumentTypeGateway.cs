using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IDocumentTypeGateway
    {
        DocumentType GetDocumentTypeByTeamNameAndDocumentId(string teamName, string documentTypeId);

        List<DocumentType> GetDocumentTypesByTeamName(string teamName);
    }
}
