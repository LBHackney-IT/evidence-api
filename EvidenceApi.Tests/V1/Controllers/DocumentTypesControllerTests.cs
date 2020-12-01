using System.Collections.Generic;
using EvidenceApi.V1.Controllers;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Controllers
{

    [TestFixture]
    public class DocumentTypesControllerTests
    {
        private DocumentTypesController _classUnderTest;
        private Mock<IDocumentTypeGateway> _mock;


        [SetUp]
        public void SetUp()
        {
            _mock = new Mock<IDocumentTypeGateway>();
            _classUnderTest = new DocumentTypesController(_mock.Object);
        }

        [Test]
        public void ReturnsAllDocumentTypes()
        {
            var docType = new DocumentType {Title = "Passport", Id = "passport"};
            var docTypes = new List<DocumentType> {docType};

            _mock.Setup(s => s.GetAll()).Returns(docTypes);

            var response = _classUnderTest.GetDocumentTypes() as OkObjectResult;

            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(docTypes);
        }
    }
}
