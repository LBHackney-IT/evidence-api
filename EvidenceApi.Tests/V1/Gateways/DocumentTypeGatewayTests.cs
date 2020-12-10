using System.Collections.Generic;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Infrastructure.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DocumentTypeGatewayTests
    {
        private DocumentTypeGateway _classUnderTest;
        private Mock<IFileReader<List<DocumentType>>> _fileReaderMock;

        [SetUp]
        public void Setup()
        {
            _fileReaderMock = new Mock<IFileReader<List<DocumentType>>>();
            _classUnderTest = new DocumentTypeGateway(_fileReaderMock.Object);
        }

        [Test]
        public void GetDocumentTypeByIdReturnsNullIfDocumentTypeDoesNotExist()
        {
            _fileReaderMock.Setup(s => s.GetData()).Returns(new List<DocumentType>());
            var response = _classUnderTest.GetDocumentTypeById("passport");

            response.Should().BeNull();
        }

        [Test]
        public void GetDocumentTypeByIdReturnsTheDocumentTypeIfItExists()
        {
            var docType = new DocumentType { Id = "passport", Title = "Passport" };
            var docTypes = new List<DocumentType> { docType };
            _fileReaderMock.Setup(s => s.GetData()).Returns(docTypes);

            var response = _classUnderTest.GetDocumentTypeById(docType.Id);

            response.Id.Should().BeSameAs(docType.Id);
            response.Title.Should().BeSameAs(docType.Title);
        }

        [Test]
        public void GetAllReturnsEmptyListIfNoData()
        {
            _fileReaderMock.Setup(s => s.GetData()).Returns(new List<DocumentType>());
            var response = _classUnderTest.GetAll();

            response.Should().BeEmpty();
        }

        [Test]
        public void GetAllReturnsDataIfDataPresent()
        {
            var docType = new DocumentType { Id = "passport", Title = "Passport" };
            var anotherDocType = new DocumentType { Id = "utility_bill", Title = "Utility Bill" };
            var docTypes = new List<DocumentType> { docType, anotherDocType };
            _fileReaderMock.Setup(s => s.GetData()).Returns(docTypes);

            var response = _classUnderTest.GetAll();
            response.Should().Contain(docTypes);
        }
    }
}
