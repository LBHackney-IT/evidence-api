using System;
using EvidenceApi.V1.Domain;
using FluentAssertions;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Domain
{
    [TestFixture]
    public class DocumentTypeTests
    {

        [Test]
        public void DocumentTypesHaveCorrectAttributes()
        {
            const string title = "Passport";
            const string id = "passport";

            var docType = new DocumentType {Title = title, Id = id};

            docType.Id.Should().BeSameAs(id);
            docType.Title.Should().BeSameAs(title);
        }
    }
}
