using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Infrastructure;

namespace EvidenceApi.V1.Gateways
{
    public class DocumentTypeGateway : IDocumentTypeGateway
    {
        private readonly IFileReader<List<DocumentType>> _reader;

        public DocumentTypeGateway(IFileReader<List<DocumentType>> reader)
        {
            _reader = reader;
        }

        public DocumentType GetDocumentTypeById(string id)
        {
            var allTypes = GetAll();
            var result = allTypes.Find(d => d.Id == id);

            return result;
        }

        public List<DocumentType> GetAll()
        {
            return _reader.GetData();
        }
    }
}
