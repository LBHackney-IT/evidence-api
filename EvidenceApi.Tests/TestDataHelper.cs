using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Infrastructure;

namespace EvidenceApi.Tests
{
    public static class TestDataHelper
    {
        private static Fixture _fixture = new Fixture();

        public static EvidenceRequest EvidenceRequest()
        {
            return _fixture.Build<EvidenceRequest>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .With(x => x.Communications, new List<Communication>())
                .With(x => x.DocumentSubmissions, new List<DocumentSubmission>())
                .With(x => x.RawDeliveryMethods, new List<string>())
                .With(x => x.Team)
                .With(x => x.NoteToResident)
                .Create();
        }

        public static DocumentSubmission DocumentSubmission(bool includeEvidenceRequest = false)
        {
            var submission = _fixture.Build<DocumentSubmission>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Without(x => x.EvidenceRequest)
                .Create();

            if (includeEvidenceRequest) submission.EvidenceRequest = EvidenceRequest();

            return submission;
        }

        public static Communication Communication()
        {
            return _fixture.Build<Communication>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Without(x => x.EvidenceRequest)
                .Create();
        }

        public static DocumentType DocumentType(string id)
        {
            var options = AppOptions.FromEnv();
            var reader = new FileReader<List<Team>>(options.DocumentTypeConfigPath);

            return reader.GetData().SelectMany(t => t.DocumentTypes).ToList().Find(dt => dt.Id == id);
        }

        public static DocumentType StaffSelectedDocumentType(string id)
        {
            var options = AppOptions.FromEnv();
            var reader = new FileReader<List<Team>>(options.StaffSelectedDocumentTypeConfigPath);

            return reader.GetData().SelectMany(t => t.DocumentTypes).ToList().Find(dt => dt.Id == id);
        }

        public static Resident Resident()
        {
            return _fixture.Build<Resident>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();
        }
    }
}
