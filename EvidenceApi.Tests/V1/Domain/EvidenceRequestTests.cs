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
    }
}
