using System;

namespace EvidenceApi.Tests
{
    public static class ConnectionString
    {
        public static string TestDatabase()
        {
            return $"{Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? "Host=localhost;Port=3002;Database=evidence_api;Username=postgres;Password=mypassword"};";
        }
    }
}
