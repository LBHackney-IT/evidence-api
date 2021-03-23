using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;
#nullable enable annotations

namespace EvidenceApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static EvidenceRequestResponse ToResponse(this EvidenceRequest domain, Resident resident, List<DocumentType> documentTypes)
        {
            return new EvidenceRequestResponse()
            {
                Resident = resident.ToResponse(),
                DeliveryMethods = domain.DeliveryMethods.ConvertAll(x => x.ToString().ToUpper()),
                DocumentTypes = documentTypes,
                ServiceRequestedBy = domain.ServiceRequestedBy,
                Reason = domain.Reason,
                UserRequestedBy = domain.UserRequestedBy,
                Id = domain.Id,
                CreatedAt = domain.CreatedAt
            };
        }

        public static ResidentResponse ToResponse(this Resident domain)
        {
            return new ResidentResponse()
            {
                Id = domain.Id,
                Name = domain.Name,
                Email = domain.Email,
                PhoneNumber = domain.PhoneNumber
            };
        }

        public static DocumentSubmissionResponse ToResponse(
            this DocumentSubmission domain,
            DocumentType documentType,
            S3UploadPolicy? s3UploadPolicy = null,
            Document? document = null
        )
        {
            return new DocumentSubmissionResponse()
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                ClaimId = domain.ClaimId,
                RejectionReason = domain.RejectionReason,
                State = domain.State.ToString().ToUpper(),
                DocumentType = documentType,
                // add staffSelectedDocumentType after DES-189
                UploadPolicy = s3UploadPolicy,
                Document = document
            };
        }
    }
}
