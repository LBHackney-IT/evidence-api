using System;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Factories;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;

namespace EvidenceApi.V1.UseCase
{
    public class CreateEvidenceRequestUseCase : ICreateEvidenceRequestUseCase
    {
        private readonly IEvidenceRequestValidator _validator;
        private readonly IDocumentTypeGateway _documentTypeGateway;
        private readonly IResidentsGateway _residentsGateway;
        private readonly IEvidenceGateway _evidenceGateway;

        public CreateEvidenceRequestUseCase(
            IEvidenceRequestValidator validator,
            IDocumentTypeGateway documentTypeGateway,
            IResidentsGateway residentsGateway,
            IEvidenceGateway evidenceGateway)
        {
            _validator = validator;
            _documentTypeGateway = documentTypeGateway;
            _residentsGateway = residentsGateway;
            _evidenceGateway = evidenceGateway;
        }

        public EvidenceRequestResponse Execute(EvidenceRequestRequest request)
        {
            var validation = _validator.Validate(request);
            if (!validation.IsValid)
            {
                throw new BadRequestException(validation);
            }

            var evidenceRequest = CreateEvidenceRequest(request);
            var created = _evidenceGateway.CreateEvidenceRequest(evidenceRequest);
            return created.ToResponse();
        }

        private EvidenceRequest CreateEvidenceRequest(EvidenceRequestRequest request)
        {
            return new EvidenceRequest
            {
                DocumentTypes = request.DocumentTypes.ConvertAll(FindDocumentType),
                DeliveryMethods = request.DeliveryMethods.ConvertAll(ParseDeliveryMethod),
                ServiceRequestedBy = request.ServiceRequestedBy,
                Resident = _residentsGateway.FindOrCreateResident(request.Resident)
            };
        }

        private DocumentType FindDocumentType(string documentTypeId)
        {
            return _documentTypeGateway.GetDocumentTypeById(documentTypeId);
        }

        private EvidenceRequest.DeliveryMethod ParseDeliveryMethod(string deliveryMethod)
        {
            return Enum.Parse<EvidenceRequest.DeliveryMethod>(deliveryMethod, true);
        }
    }
}
