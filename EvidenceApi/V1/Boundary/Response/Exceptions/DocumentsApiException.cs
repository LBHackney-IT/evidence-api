using System;

namespace EvidenceApi.V1.Boundary.Response.Exceptions
{
    public class DocumentsApiException : Exception
    {
        public DocumentsApiException(string message) : base(message)
        {
        }
    }
}
