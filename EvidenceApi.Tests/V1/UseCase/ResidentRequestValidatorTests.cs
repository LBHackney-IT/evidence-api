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

        #region Email Validations

        [Test]
        public void ValidatesEmail()
        {
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Email, _request);
        }

        [Test]
        public void IsInvalidWhenEmailIsNotAnEmail()
        {
            _request.Email = "not a valid email";
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Email, _request)
                .WithErrorMessage("'Email' is not a valid email address.");
        }

        [Test]
        public void IsInvalidWhenEmailIsEmpty()
        {
            _request.Email = "";
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Email, _request)
                .WithErrorMessage("'Email' is not a valid email address.");
        }

        [Test]
        public void IsInvalidWhenEmailIsNull()
        {
            _request.Email = null;
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Email, _request)
                .WithErrorMessage("'Email' must not be empty.");
        }

        #endregion

        #region Phone Number Validations

        [Test]
        public void ValidatesPhoneNumber()
        {
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber, _request);

            _request.PhoneNumber = "";
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.PhoneNumber, _request)
                .WithErrorMessage("'Phone Number' must not be empty.");
        }

        public void IsInvalidWhenPhoneNumberIsEmpty()
        {
            _request.PhoneNumber = "";
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.PhoneNumber, _request)
                .WithErrorMessage("'Phone Number' must not be empty.");
        }

        #endregion

        private static ResidentRequest CreateRequest()
        {
            return new ResidentRequest()
            {
                Name = "Tom",
                Email = "tom@hackney.gov.uk",
                PhoneNumber = "+447123456789"
            };
        }
    }
}
