using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Domain;
using System;
#nullable enable annotations

namespace EvidenceApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static EvidenceRequestResponse ToResponse(this EvidenceRequest domain, Resident resident, List<DocumentType> documentTypes)
        {
            return new EvidenceRequestResponse()
            {
                Resident = resident.ToResponse(domain.ResidentReferenceId),
                DeliveryMethods = domain.DeliveryMethods.ConvertAll(x => x.ToString().ToUpper()),
                DocumentTypes = documentTypes,
                Team = domain.Team,
                Reason = domain.Reason,
                UserRequestedBy = domain.UserRequestedBy,
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                NoteToResident = domain.NoteToResident
            };
        }

        public static ResidentResponse ToResponse(this Resident domain, string? referenceId = null)
        {
            return new ResidentResponse()
            {
                Id = domain.Id,
                Name = domain.Name,
                Email = domain.Email,
                PhoneNumber = domain.PhoneNumber,
                ReferenceId = referenceId
            };
        }

        public static DocumentSubmissionResponse ToResponse(
            this DocumentSubmission domain,
            DocumentType documentType,
            Guid evidenceRequestId,
            DocumentType? staffSelectedDocumentType = null,
            S3UploadPolicy? s3UploadPolicy = null,
            Claim? claim = null
        )
        {
            return claim == null ? new DocumentSubmissionResponse()
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                ClaimId = domain.ClaimId,
                AcceptedAt = domain.AcceptedAt,
                RejectionReason = domain.RejectionReason,
                RejectedAt = domain.RejectedAt,
                UserUpdatedBy = domain.UserUpdatedBy,
                State = domain.State.ToString().ToUpper(),
                DocumentType = documentType,
                EvidenceRequestId = evidenceRequestId,
                StaffSelectedDocumentType = staffSelectedDocumentType,
                UploadPolicy = s3UploadPolicy,
                Team = domain.Team
            } : new DocumentSubmissionResponse()
            {
                Id = domain.Id,
                CreatedAt = domain.CreatedAt,
                ClaimId = domain.ClaimId,
                AcceptedAt = domain.AcceptedAt,
                RejectionReason = domain.RejectionReason,
                RejectedAt = domain.RejectedAt,
                UserUpdatedBy = domain.UserUpdatedBy,
                State = domain.State.ToString().ToUpper(),
                DocumentType = documentType,
                EvidenceRequestId = evidenceRequestId,
                StaffSelectedDocumentType = staffSelectedDocumentType,
                UploadPolicy = s3UploadPolicy,
                Document = claim.Document,
                ClaimValidUntil = claim.ValidUntil,
                RetentionExpiresAt = claim.RetentionExpiresAt,
                Team = domain.Team
            };
        }
    }
}
