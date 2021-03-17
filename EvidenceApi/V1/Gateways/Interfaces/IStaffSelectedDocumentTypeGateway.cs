using System.Collections.Generic;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.Gateways.Interfaces
{
    public interface IStaffSelectedDocumentTypeGateway
    {
        StaffSelectedDocumentType GetStaffSelectedDocumentTypeById(string id);

        List<StaffSelectedDocumentType> GetAll();
    }
}
