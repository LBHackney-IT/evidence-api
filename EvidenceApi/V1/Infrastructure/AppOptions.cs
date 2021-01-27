using System;
using System.IO;

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
            var documentsApiUrl = Environment.GetEnvironmentVariable("DOCUMENTS_API_URL");
            var evidenceApiUrl = Environment.GetEnvironmentVariable("EVIDENCE_REQUEST_CLIENT_URL");
            return new AppOptions()
            {
                NotifyApiKey = Environment.GetEnvironmentVariable("NOTIFY_API_KEY"),
                DocumentTypeConfigPath = Path.Combine(Environment.CurrentDirectory, @"DocumentTypes.json"),
                DatabaseConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING"),
                EvidenceRequestClientUrl = evidenceApiUrl != null ? new Uri(evidenceApiUrl) : null,
                DocumentsApiUrl = documentsApiUrl != null ? new Uri(documentsApiUrl) : null,
                DocumentsApiPostClaimsToken = Environment.GetEnvironmentVariable("DOCUMENTS_API_POST_CLAIMS_TOKEN"),
                DocumentsApiPostDocumentsToken = Environment.GetEnvironmentVariable("DOCUMENTS_API_POST_DOCUMENTS_TOKEN")
            };
        }
    }
}
