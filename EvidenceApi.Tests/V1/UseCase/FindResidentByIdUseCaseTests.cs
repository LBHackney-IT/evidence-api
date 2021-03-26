using System;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class FindResidentByIdUseCaseTests
    {
        private FindResidentByIdUseCase _classUnderTest;
        private Mock<IResidentsGateway> _residentsGateway;

        [SetUp]
        public void SetUp()
        {
            _residentsGateway = new Mock<IResidentsGateway>();
            _classUnderTest = new FindResidentByIdUseCase(_residentsGateway.Object);
        }

        [Test]
        public void ReturnsTheFoundEvidenceRequest()
        {
            var resident = TestDataHelper.Resident();

            var id = new Guid();
            _residentsGateway.Setup(x => x.FindResident(id)).Returns(resident);
            var result = _classUnderTest.Execute(id);

            result.Should().BeEquivalentTo(resident, opts => opts.Excluding(x => x.CreatedAt));
        }

        [Test]
        public void ThrowsAnErrorWhenAnEvidenceRequestIsNotFound()
        {
            var id = new Guid();
            Action act = () => _classUnderTest.Execute(id);
            act.Should().Throw<NotFoundException>().WithMessage($"Could not find resident with id {id}");
        }
    }
}
