using System;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain.Enums;
using System.Linq;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;

namespace EvidenceApi.V1.UseCase
{
    public class UpdateEvidenceRequestStateUseCase : IUpdateEvidenceRequestStateUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;

        public UpdateEvidenceRequestStateUseCase(IEvidenceGateway evidenceGateway)
        {
            _evidenceGateway = evidenceGateway;
        }

        public EvidenceRequest Execute(Guid id)
        {
            var evidenceRequest = _evidenceGateway.FindEvidenceRequestWithDocumentSubmissions(id);

            if (evidenceRequest == null)
            {
                throw new NotFoundException($"Cannot find an evidence request with ID: {id}");
            }

            if (AllDocumentSubmissionsAreApproved(evidenceRequest))
            {
                evidenceRequest.State = EvidenceRequestState.Approved;
            }
            else
            {
                if (AtLeastOneDocumentSubmissionIsUploaded(evidenceRequest))
                {
                    evidenceRequest.State = EvidenceRequestState.ForReview;
                }
                else
                {
                    evidenceRequest.State = EvidenceRequestState.Pending;
                }
            }
            _evidenceGateway.CreateEvidenceRequest(evidenceRequest);
            return evidenceRequest;
        }

        private static bool AllDocumentSubmissionsAreApproved(EvidenceRequest evidenceRequest)
        {
            return evidenceRequest.DocumentSubmissions.All(ds => ds.State == SubmissionState.Approved);
        }

        private static bool AtLeastOneDocumentSubmissionIsUploaded(EvidenceRequest evidenceRequest)
        {
            return evidenceRequest.DocumentSubmissions.Any(ds => ds.State == SubmissionState.Uploaded);
        }
    }
}
