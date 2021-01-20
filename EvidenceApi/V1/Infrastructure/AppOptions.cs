using System;
using System.IO;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Infrastructure
{
    public class AppOptions
    {
        public string NotifyApiKey { get; set; }
        public string DocumentTypeConfigPath { get; set; }
        public string DatabaseConnectionString { get; set; }
        public Uri EvidenceRequestClientUrl { get; set; }
        public Uri DocumentsApiUrl { get; set; }
        public string DocumentsApiPostClaimsToken { get; set; }
        public string DocumentsApiPostDocumentsToken { get; set; }

        public static AppOptions FromEnv()
        {
            return new AppOptions()
            {
                NotifyApiKey = Environment.GetEnvironmentVariable("NOTIFY_API_KEY"),
                DocumentTypeConfigPath = Path.Combine(Environment.CurrentDirectory, @"DocumentTypes.json"),
                DatabaseConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING"),
                EvidenceRequestClientUrl = new Uri(Environment.GetEnvironmentVariable("EVIDENCE_REQUEST_CLIENT_URL")!),
                DocumentsApiUrl = new Uri(Environment.GetEnvironmentVariable("DOCUMENTS_API_URL")!),
                DocumentsApiPostClaimsToken = Environment.GetEnvironmentVariable("DOCUMENTS_API_POST_CLAIMS_TOKEN"),
                DocumentsApiPostDocumentsToken = Environment.GetEnvironmentVariable("DOCUMENTS_API_POST_DOCUMENTS_TOKEN")
            };
        }
    }
}
