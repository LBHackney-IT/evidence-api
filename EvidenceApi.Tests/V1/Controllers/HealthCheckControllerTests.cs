using System.Collections.Generic;
using EvidenceApi.V1.Controllers;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using EvidenceApi.V1.UseCase.Interfaces;
using Moq;

namespace EvidenceApi.Tests.V1.Controllers
{

    [TestFixture]
    public class HealthCheckControllerTests
    {
        private Mock<ICreateAuditUseCase> _mockCreateAuditUseCase;
        private HealthCheckController _classUnderTest;


        [SetUp]
        public void SetUp()
        {
            _mockCreateAuditUseCase = new Mock<ICreateAuditUseCase>();
            _classUnderTest = new HealthCheckController(_mockCreateAuditUseCase.Object);
        }

        [Test]
        public void ReturnsResponseWithStatus()
        {
            var response = _classUnderTest.HealthCheck() as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
        }

        [Test]
        public void ThrowErrorThrows()
        {
            Assert.Throws<TestOpsErrorException>(_classUnderTest.ThrowError);
        }
    }
}
