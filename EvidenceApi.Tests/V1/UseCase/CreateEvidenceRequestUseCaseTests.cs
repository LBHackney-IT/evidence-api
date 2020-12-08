using System;
using System.Threading.Tasks;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Boundary.Response;
using EvidenceApi.V1.Boundary.Response.Exceptions;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.UseCase.Interfaces;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class CreateEvidenceRequestUseCaseTests
    {
        private CreateEvidenceRequestUseCase _classUnderTest;
        private Mock<IEvidenceRequestValidator> _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<IEvidenceRequestValidator>();
            _classUnderTest = new CreateEvidenceRequestUseCase(_validator.Object);
        }

        [Test]
        public void ThrowsBadRequestErrorWhenRequestIsInvalid()
        {
            SetupValidatorToReturn(false);
            Func<Task<EvidenceRequestResponse>> testDelegate = async () => await _classUnderTest.ExecuteAsync(new EvidenceRequestRequest()).ConfigureAwait(true);
            testDelegate.Should().Throw<BadRequestException>();
        }

        private void SetupValidatorToReturn(bool valid = true)
        {
            var result = new Mock<ValidationResult>();
            result.Setup(x => x.IsValid).Returns(valid);
            _validator.Setup(x => x.Validate(It.IsAny<EvidenceRequestRequest>()))
                .Returns(result.Object);
        }
    }
}
