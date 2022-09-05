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
            const string description = "Please have both sides scanned";
            const bool enabled = true;

            var docType = new DocumentType { Title = title, Id = id, Description = description, Enabled = enabled };

            docType.Id.Should().BeSameAs(id);
            docType.Title.Should().BeSameAs(title);
            docType.Description.Should().BeSameAs(description);
            docType.Enabled.Should().BeTrue();
        }
    }
}
