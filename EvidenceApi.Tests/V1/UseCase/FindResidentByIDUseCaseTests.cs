using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class FindResidentByIDUseCaseTests
    {
        private FindResidentByIDUseCase _classUnderTest;
        private Mock<IResidentsGateway> _residentsGateway;

        [SetUp]
        public void SetUp()
        {
            _residentsGateway = new Mock<IResidentsGateway>();
            _classUnderTest = new FindResidentByIDUseCase(_residentsGateway.Object);
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
