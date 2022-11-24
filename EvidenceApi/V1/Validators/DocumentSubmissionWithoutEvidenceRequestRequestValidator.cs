using FluentValidation;
using EvidenceApi.V1.Boundary.Request;
using System;

namespace EvidenceApi.V1.Validators
{
    public class DocumentSubmissionWithoutEvidenceRequestRequestValidator : AbstractValidator<DocumentSubmissionWithoutEvidenceRequestRequest>
    {
        public DocumentSubmissionWithoutEvidenceRequestRequestValidator()
        {
            RuleFor(x => x.Team).NotEmpty();
            RuleFor(x => x.UserCreatedBy).NotEmpty();
            RuleFor(x => x.StaffSelectedDocumentTypeId).NotEmpty();
            RuleFor(x => x.DocumentDescription).NotEmpty();
        }
    }
}
