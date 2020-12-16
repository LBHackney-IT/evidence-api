using System;
using EvidenceApi.V1.Domain;
using FluentValidation.Results;

namespace EvidenceApi.V1.Boundary.Response.Exceptions
{
    public class NotificationException : Exception
    {
        private readonly EvidenceRequest _evidenceRequest;

        public NotificationException(EvidenceRequest evidenceRequest) : base("Evidence request created, but something went wrong sending a notification using .Gov Notify")
        {
            _evidenceRequest = evidenceRequest;
        }
    }
}
