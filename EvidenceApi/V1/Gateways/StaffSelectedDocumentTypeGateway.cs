using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Gateways
{
    public class StaffSelectedDocumentTypeGateway : IStaffSelectedDocumentTypeGateway
    {
        private readonly IFileReader<List<StaffSelectedDocumentType>> _reader;

        public StaffSelectedDocumentTypeGateway(IFileReader<List<StaffSelectedDocumentType>> reader)
        {
            _reader = reader;
        }

        public StaffSelectedDocumentType GetStaffSelectedDocumentTypeById(string id)
        {
            var allTypes = GetAll();
            var result = allTypes.Find(d => d.Id == id);

            return result;
        }

        public List<StaffSelectedDocumentType> GetAll()
        {
            return _reader.GetData();
        }
    }
}
