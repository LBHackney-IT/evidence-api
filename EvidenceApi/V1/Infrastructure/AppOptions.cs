using System;
using System.IO;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Infrastructure
{
    public static class AppOptions
    {
        public static string NotifyApiKey => Environment.GetEnvironmentVariable("NOTIFY_API_KEY");
        public static string DocumentTypeConfigPath => Path.Combine(Environment.CurrentDirectory, @"DocumentTypes.json");
        public static string DatabaseConnectionString => Environment.GetEnvironmentVariable("CONNECTION_STRING");
        public static Uri EvidenceRequestClientUrl => new Uri(Environment.GetEnvironmentVariable("EVIDENCE_REQUEST_CLIENT_URL")!);
        public static Uri DocumentsApiUrl => new Uri(Environment.GetEnvironmentVariable("DOCUMENTS_API_URL")!);
    }
}
