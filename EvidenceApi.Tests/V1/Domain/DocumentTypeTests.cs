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
            var docType = new DocumentType();
            const string title = "Passport";
            const string id = "passport";
            docType.Title = title;
            docType.Id = id;

            docType.Id.Should().BeSameAs(id);
            docType.Title.Should().BeSameAs(title);
        }
    }
}
