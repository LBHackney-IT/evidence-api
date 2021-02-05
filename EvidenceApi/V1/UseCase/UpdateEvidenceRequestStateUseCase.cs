using System;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain.Enums;
using System.Linq;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Boundary.Response.Exceptions;

namespace EvidenceApi.V1.UseCase
{
    public class UpdateEvidenceRequestStateUseCase : IUpdateEvidenceRequestStateUseCase
    {
        private readonly IEvidenceGateway _evidenceGateway;
        private readonly IDocumentTypeGateway _documentTypeGateway;

        public UpdateEvidenceRequestStateUseCase(IEvidenceGateway evidenceGateway, IDocumentTypeGateway documentTypeGateway)
        {
            _evidenceGateway = evidenceGateway;
            _documentTypeGateway = documentTypeGateway;
        }

        public EvidenceRequestState Execute(Guid id)
        {
            var evidenceRequest = _evidenceGateway.FindEvidenceRequest(id);
            if (evidenceRequest == null)
            {
                throw new NotFoundException($"Cannot find an evidence request with ID: {id}");
            }

            if (evidenceRequest.DocumentTypes.ToArray().All(dt =>
                evidenceRequest.DocumentSubmissions.Any(ds =>
                ds.State == SubmissionState.Approved && ds.DocumentTypeId == dt)
            ))
            {
                return EvidenceRequestState.Approved;
            }
            else
            {
                if (evidenceRequest.DocumentTypes.ToArray().All(dt =>
                    evidenceRequest.DocumentSubmissions.Any(ds =>
                    ds.State == SubmissionState.Uploaded && ds.DocumentTypeId == dt)
                ))
                {
                    return EvidenceRequestState.ForReview;
                }
            }
            return EvidenceRequestState.Pending;
        }
    }
}
