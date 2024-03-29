using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.UseCase;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class ResidentRequestValidatorTests
    {
        private ResidentRequestValidator _classUnderTest;
        private ResidentRequest _request;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new ResidentRequestValidator();
            _request = CreateRequest();
        }

        #region Name Validations

        [Test]
        public void ValidatesName()
        {
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Name, _request);
        }

        [Test]
        public void IsInvalidWhenNameIsEmpty()
        {

            _request.Name = "";
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Name, _request)
                .WithErrorMessage("'Name' must not be empty.");
        }

        #endregion

        #region Email and Phone Number Validations

        [Test]
        public void IsInvalidWhenEmailAndPhoneNumberAreEmpty()
        {
            _request.Email = "";
            _request.PhoneNumber = "";
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Email, _request)
                .WithErrorMessage("'Email' and 'Phone number' cannot be both empty.");
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.PhoneNumber, _request)
                .WithErrorMessage("'Email' and 'Phone number' cannot be both empty.");
        }

        #endregion

        private static ResidentRequest CreateRequest()
        {
            return new ResidentRequest()
            {
                Name = "Tom",
                Email = "tom@hackney.gov.uk",
                PhoneNumber = "+447123456789",
                Team = "some team"
            };
        }
    }
}
