using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentValidation;

namespace EvidenceApi.V1.UseCase
{
    public class EvidenceRequestValidator : AbstractValidator<EvidenceRequestRequest>, IEvidenceRequestValidator
    {
        private readonly List<DocumentType> _documentTypes;

        public EvidenceRequestValidator(IValidator<ResidentRequest> residentValidator, IDocumentTypeGateway documentTypeGateway)
        {
            _documentTypes = documentTypeGateway.GetAll();

            RuleFor(x => x.ServiceRequestedBy).NotEmpty();

            RuleFor(x => x.DocumentTypes).NotEmpty();
            RuleForEach(x => x.DocumentTypes)
                .Must(CheckForValidDocumentTypes)
                .WithMessage("'Document Types' must only contain valid document type IDs.");

            RuleFor(x => x.DeliveryMethods).NotNull();
            RuleForEach(x => x.DeliveryMethods)
                .IsEnumName(typeof(DeliveryMethod), false);

            RuleFor(x => x.Resident)
                .NotEmpty()
                .SetValidator(residentValidator);
        }

        private bool CheckForValidDocumentTypes(string documentTypeId)
        {
            return _documentTypes.Exists(dt => dt.Id == documentTypeId);
        }
    }
}
