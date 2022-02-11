using System.Collections.Generic;

namespace EvidenceApi
{
    public static class AcceptedMimeTypes
    {
        public readonly static List<string> acceptedMimeTypes = new List<string> {
            "application/msword",                                                       //.doc
            "application/pdf",                                                          //.pdf
            "application/vnd.apple.numbers",                                            //.numbers
            "application/vnd.apple.pages",                                              //.pages
            "application/vnd.ms-excel",                                                 //.xls
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",        //.xlsx
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",  //.docx
            "image/bmp",                                                                //.bmp
            "image/gif",                                                                //.gif
            "image/heic",                                                               //.heic
            "image/jpeg",                                                               //.jpeg or .jpg
            "image/png",                                                                //.png
            "text/plain",                                                               //.txt
            //"video/3gpp",                                                             //.3gpp or .3gp
            //"video/mp4",                                                              //.mp4
            //"video/quicktime",                                                        //.mov or .qt
            };
    }
}
