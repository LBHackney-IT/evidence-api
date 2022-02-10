using System.Collections.Generic;

namespace EvidenceApi
{
    public static class AcceptedMimeTypes
    {
        public readonly static List<string> acceptedMimeTypes = new List<string> {
            "application/msword",
            "application/octet-stream",
            "application/pdf",
            "application/vnd.apple.numbers",
            "application/vnd.apple.pages",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "image/bmp",
            "image/gif",
            "image/heic",
            "image/jpeg",
            "image/png",
            "text/plain",
            "video/3gpp",
            "video/mp4",
            "video/quicktime",
            };
    }
}
