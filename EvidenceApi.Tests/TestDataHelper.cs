using System.Collections.Generic;
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
                .Without(x => x.Communications)
                .Without(x => x.DocumentSubmissions)
                .Without(x => x.RawDeliveryMethods)
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
            var reader = new FileReader<List<DocumentType>>(options.DocumentTypeConfigPath);

            return reader.GetData().Find(x => x.Id == id);
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
