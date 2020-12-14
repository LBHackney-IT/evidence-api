using System;
using EvidenceApi.V1.Infrastructure.Interfaces;
using Notify.Client;
using Notify.Interfaces;

namespace EvidenceApi.V1.Gateways
{
    public class NotifyGateway
    {
        private INotificationClient _client;
        public NotifyGateway(INotificationClient client)
        {
            _client = client;
        }
    }
}
