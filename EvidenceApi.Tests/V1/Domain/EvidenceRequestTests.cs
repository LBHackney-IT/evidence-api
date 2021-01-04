using System;
using AutoFixture;
using EvidenceApi.V1.Domain;
using FluentAssertions;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Domain
{
    [TestFixture]
    public class EvidenceRequestTests
    {
        private EvidenceRequest _classUnderTest;
        private Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = _fixture.Create<EvidenceRequest>();
        }

        [Test]
        public void HasAMagicLink()
        {
            _classUnderTest.Id = Guid.Parse("c021e098-ccd5-4ad8-adb3-c64aeb1c1357");
            var clientUrl = Environment.GetEnvironmentVariable("EVIDENCE_REQUEST_CLIENT_URL");
            _classUnderTest.MagicLink.Should().Be($"{clientUrl}/c021e098-ccd5-4ad8-adb3-c64aeb1c1357");
        }
    }
}
