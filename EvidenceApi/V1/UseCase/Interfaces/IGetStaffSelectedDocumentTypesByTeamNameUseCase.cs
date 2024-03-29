using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase.Interfaces
{
    public interface IGetStaffSelectedDocumentTypesByTeamNameUseCase
    {
        List<DocumentType> Execute(string team, bool? enabled);
    }
}
