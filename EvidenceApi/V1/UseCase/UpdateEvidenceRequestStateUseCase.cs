using System;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Domain.Enums;
using System.Linq;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using System.Collections.Generic;

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
                evidenceRequest.State = EvidenceRequestState.Approved;
            }
            else
            {
                if (evidenceRequest.DocumentTypes.ToArray().Any(dt =>
                    evidenceRequest.DocumentSubmissions.Any(ds =>
                    ds.State == SubmissionState.Uploaded && ds.DocumentTypeId == dt)
                ))
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
    }
}
