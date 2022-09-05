using EvidenceApi.V1.Domain;
using System.Collections.Generic;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IGetDocumentTypesByTeamNameUseCase
    {
        List<DocumentType> Execute(string team, bool? enabled);
    }
}
