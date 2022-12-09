using System;
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
                .With(x => x.isHidden, false)
                .Create();

            if (includeEvidenceRequest) submission.EvidenceRequest = EvidenceRequest();

            return submission;
        }

        public static DocumentSubmission DocumentSubmissionWithResidentId(Guid residentId, EvidenceRequest evidenceRequest)
        {
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior()); ;

            var submission = _fixture.Build<DocumentSubmission>()
                .With(x => x.ResidentId, residentId)
                .With(x => x.EvidenceRequest, evidenceRequest)
                .With(x => x.EvidenceRequestId, evidenceRequest.Id)
                .With(x => x.isHidden, false)
                .Without(x => x.CreatedAt)
                .Create();



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

        public static List<DocumentType> GetDistinctDocumentTypes()
        {
            var options = AppOptions.FromEnv();
            var reader = new FileReader<List<Team>>(options.DocumentTypeConfigPath);


            var listOfAllDocumentTypes = reader.GetData().SelectMany(team => team.DocumentTypes).ToList();
            var distinctDocumentTypesById = listOfAllDocumentTypes.GroupBy(documentType => documentType.Id)
                .Select(documentType => documentType.First()).ToList();
            return distinctDocumentTypesById;
        }

        public static DocumentType StaffSelectedDocumentType(string id)
        {
            var options = AppOptions.FromEnv();
            var reader = new FileReader<List<Team>>(options.StaffSelectedDocumentTypeConfigPath);

            return reader.GetData().SelectMany(t => t.DocumentTypes).ToList().Find(dt => dt.Id == id);
        }

        public static DocumentType GetStaffSelectedDocumentTypeByTeamName(string id, string teamName)
        {
            var options = AppOptions.FromEnv();
            var reader = new FileReader<List<Team>>(options.StaffSelectedDocumentTypeConfigPath);

            var teamDocTypes = reader.GetData().Where(t => t.Name == teamName).SelectMany(t => t.DocumentTypes).ToList();
            var docType = teamDocTypes.Find(dt => dt.Id == id);
            return docType;
        }

        public static Resident Resident()
        {
            return _fixture.Build<Resident>()
                .Without(x => x.Id)
                .Without(x => x.CreatedAt)
                .Create();
        }

        public static Resident ResidentWithId(Guid id)
        {
            return _fixture.Build<Resident>()
                .With(x => x.Id, id)
                .Create();
        }
    }
}
