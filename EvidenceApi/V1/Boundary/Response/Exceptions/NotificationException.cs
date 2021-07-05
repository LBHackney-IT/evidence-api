using System;

namespace EvidenceApi.V1.Boundary.Response.Exceptions
{
    public class NotificationException : Exception
    {
        public NotificationException(string message) : base(message)
        {

        }
    }
}
