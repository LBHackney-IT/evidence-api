using System;
using System.Collections.Generic;

namespace EvidenceApi.V1.Domain
{
    public class S3UploadPolicy
    {
        public Uri Url { get; set; }
        public Dictionary<string, string> Fields { get; set; }
    }
}
