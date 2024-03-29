using System;
using FluentValidation.Results;

namespace EvidenceApi.V1.Boundary.Response.Exceptions
{
    public class BadRequestException : Exception
    {
        public ValidationResult ValidationResponse { get; set; }

        public BadRequestException(ValidationResult validationResponse)
        {
            ValidationResponse = validationResponse;
        }

        public BadRequestException(string message) : base(message)
        {
        }
    }
}
