using System.Collections.Generic;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.UseCase;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.UseCase
{
    public class EvidenceRequestValidatorTests
    {
        private EvidenceRequestValidator _classUnderTest;
        private EvidenceRequestRequest _request;
        private ResidentRequestValidator _residentValidator;

        [SetUp]
        public void SetUp()
        {
            _residentValidator = new ResidentRequestValidator();
            _classUnderTest = new EvidenceRequestValidator(_residentValidator);
            _request = CreateRequest();
        }

        #region DeliveryMethods Validations

        [Test]
        public void ValidatesIncorrectDeliveryMethods()
        {
            _request.DeliveryMethods = new List<string>() { "FOO" };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.DeliveryMethods, _request)
                .WithErrorMessage("'Delivery Methods' has a range of values which does not include 'FOO'.");
        }

        [Test]
        public void IsInvalidWhenDeliveryMethodsIsNull()
        {
            _request.DeliveryMethods = null;
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.DeliveryMethods, _request)
                .WithErrorMessage("'Delivery Methods' must not be empty.");
        }

        [Test]
        public void IsValidWhenDeliveryMethodsIsEmpty()
        {
            _request.DeliveryMethods = new List<string>();
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.DeliveryMethods, _request);
        }

        [TestCase("EMAIL")]
        [TestCase("email")]
        [TestCase("Email")]
        public void ValidatesDeliveryMethodCaseInsensitively(string deliverMethodValue)
        {
            _request.DeliveryMethods = new List<string>() { deliverMethodValue };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.DeliveryMethods, _request);
        }

        #endregion

        #region DocumentType Validations

        [Test]
        public void IsInvalidWhenDocumentTypeIsNull()
        {
            _request.DocumentType = null;
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.DocumentType, _request)
                .WithErrorMessage("'Document Type' must not be empty.");
        }

        [Test]
        public void IsInvalidWhenDocumentTypeIsEmpty()
        {
            _request.DocumentType = "";
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.DocumentType, _request)
                .WithErrorMessage("'Document Type' must not be empty.");
        }
        [Test]
        public void IsValidWhenDocumentTypeIsPresent()
        {
            _request.DocumentType = "passport-scan";
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.DocumentType, _request);
        }

        #endregion

        #region ServiceRequestedBy Validations

        [Test]
        public void IsInvalidWhenServiceRequestedByIsNull()
        {
            _request.ServiceRequestedBy = null;
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ServiceRequestedBy, _request)
                .WithErrorMessage("'Service Requested By' must not be empty.");
        }

        [Test]
        public void IsInvalidWhenServiceRequestedByIsEmpty()
        {
            _request.ServiceRequestedBy = "";
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ServiceRequestedBy, _request)
                .WithErrorMessage("'Service Requested By' must not be empty.");
        }
        [Test]
        public void IsValidWhenServiceRequestedByIsPresent()
        {
            _request.ServiceRequestedBy = "housing_needs";
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.ServiceRequestedBy, _request);
        }

        #endregion

        #region Resident Validations

        [Test]
        public void IsValidWhenResidentIsValid()
        {
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Resident, _request);
        }

        [Test]
        public void IsInvalidWhenResidentIsNotValid()
        {
            _request.Resident.Name = null;
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Resident.Name, _request)
                .WithErrorMessage("'Name' must not be empty.");

            _classUnderTest.Validate(_request).IsValid.Should().BeFalse();
        }

        [Test]
        public void IsInvalidWhenResidentIsNotPresent()
        {
            _request.Resident = null;
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Resident, _request)
                .WithErrorMessage("'Resident' must not be empty.");
        }


        #endregion

        private static EvidenceRequestRequest CreateRequest()
        {
            return new EvidenceRequestRequest()
            {
                ServiceRequestedBy = "housing_needs",
                DeliveryMethods = new List<string>() { "EMAIL", "SMS" },
                Resident = new ResidentRequest()
                {
                    Name = "Tom",
                    Email = "tom@hackney.gov.uk",
                    PhoneNumber = "+447123456789"
                },
                DocumentType = "passport-scan"
            };
        }
    }
}
