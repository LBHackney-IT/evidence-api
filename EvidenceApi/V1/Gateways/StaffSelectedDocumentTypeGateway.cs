using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Gateways
{
    public class StaffSelectedDocumentTypeGateway : IStaffSelectedDocumentTypeGateway
    {
        private readonly IFileReader<List<Team>> _reader;

        public StaffSelectedDocumentTypeGateway(IFileReader<List<Team>> reader)
        {
            _reader = reader;
        }

        public DocumentType GetDocumentTypeByTeamNameAndDocumentTypeId(string teamName, string documentTypeId)
        {
            var documentTypesForTeam = GetDocumentTypesByTeamName(teamName);
            var result = documentTypesForTeam.Find(d => d.Id == documentTypeId);

            return result;
        }

        public List<DocumentType> GetDocumentTypesByTeamName(string teamName)
        {
            var teamFromFile = _reader.GetData().Find(t => t.Name == teamName);
            if (teamFromFile == null)
            {
                return new List<DocumentType>();
            }
            return teamFromFile.DocumentTypes;
        }

        public string GetTeamIdByTeamName(string teamName)
        {
            var teamFromFile = _reader.GetData().Find(t => t.Name == teamName);
            if (teamFromFile == null)
            {
                return "";
            }
            return teamFromFile.Id;
        }
    }
}
