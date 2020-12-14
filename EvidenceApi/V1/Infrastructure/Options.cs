using System;
using System.IO;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Infrastructure
{
    public class Options : IOptions
    {
        public string NotifyApiKey => Environment.GetEnvironmentVariable("NOTIFY_API_KEY");
        public string DocumentTypeConfigPath => Path.Combine(Environment.CurrentDirectory, @"DocumentTypes.json");
        public string DatabaseConnectionString => Environment.GetEnvironmentVariable("CONNECTION_STRING");
    }
}
