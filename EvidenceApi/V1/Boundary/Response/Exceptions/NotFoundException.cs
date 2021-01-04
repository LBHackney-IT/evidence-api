using System;
using FluentValidation.Results;

namespace EvidenceApi.V1.Boundary.Response.Exceptions
{
    public class NotFoundException : Exception
    {

        public NotFoundException(string message) : base(message) { }
    }
}
