using System.Collections.Generic;
using DocumentsApi.V1.Controllers;
using DocumentsApi.V1.UseCase;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace DocumentsApi.Tests.V1.Controllers
{

    [TestFixture]
    public class HealthCheckControllerTests
    {
        private HealthCheckController _classUnderTest;


        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new HealthCheckController();
        }

        [Test]
        public void ReturnsResponseWithStatus()
        {
            var expected = new Dictionary<string, object> { { "success", true } };
            var response = _classUnderTest.HealthCheck() as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ThrowErrorThrows()
        {
            Assert.Throws<TestOpsErrorException>(_classUnderTest.ThrowError);
        }
    }
}
