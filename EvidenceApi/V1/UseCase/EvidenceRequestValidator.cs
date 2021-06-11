using System;
using System.Linq;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Domain.Enums;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentValidation;

namespace EvidenceApi.V1.UseCase
{
    public class EvidenceRequestValidator : AbstractValidator<EvidenceRequestRequest>, IEvidenceRequestValidator
    {
        public EvidenceRequestValidator(IValidator<ResidentRequest> residentValidator, IDocumentTypeGateway documentTypeGateway)
        {
            RuleFor(x => x.Team).NotEmpty();
            RuleFor(x => x.Team).NotNull();

            RuleFor(x => x.DocumentTypes).NotEmpty();
            RuleFor(x => x.DocumentTypes).NotNull();

            RuleFor(x => x)
                .Custom((evidenceReqReq, context) =>
                {
                    if (String.IsNullOrEmpty(evidenceReqReq.Team) || evidenceReqReq.DocumentTypes == null)
                    {
                        return;
                    }
                    var documentTypesForTeam = documentTypeGateway.GetDocumentTypesByTeamName(evidenceReqReq.Team).Select(dt => dt.Id);
                    var documentTypesInRequestAndTeamLookup = evidenceReqReq.DocumentTypes.Intersect(documentTypesForTeam);
                    var allRequestDocumentTypesFoundInTeamDocumentTypes = documentTypesInRequestAndTeamLookup.Count() == evidenceReqReq.DocumentTypes.Count;
                    if (!allRequestDocumentTypesFoundInTeamDocumentTypes)
                    {
                        context.AddFailure("DocumentTypes", "'Document Types' must only contain valid document type IDs.");
                    }
                });

            RuleFor(x => x.DeliveryMethods).NotNull();
            RuleForEach(x => x.DeliveryMethods)
                .IsEnumName(typeof(DeliveryMethod), false);

            RuleFor(x => x.Resident)
                .NotEmpty()
                .SetValidator(residentValidator);
        }
    }
}
