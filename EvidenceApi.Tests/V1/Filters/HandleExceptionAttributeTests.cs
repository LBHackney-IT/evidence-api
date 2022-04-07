using System;
using System.Collections.Generic;
using EvidenceApi.V1.Filters;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace EvidenceApi.Tests.V1.Filters
{
    public class HandleExceptionAttributeTests
    {
        private readonly Mock<ILogger<HandleExceptionAttribute>> _logger;
        private readonly ApiBehaviorOptions _options;
        private readonly HandleExceptionAttribute _classUnderTest;

        public HandleExceptionAttributeTests()
        {
            _logger = new Mock<ILogger<HandleExceptionAttribute>>();
            _options = new ApiBehaviorOptions();
            _classUnderTest = new HandleExceptionAttribute(_logger.Object, Options.Create(_options));
        }

        [Test]
        public void ContextHasCorrectResponse()
        {
            var actionContext = new ActionContext
            {
                ActionDescriptor = new ActionDescriptor(),
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData()
            };
            const string message = "Test message";
            var exception = new Exception(message);
            var exceptionContext = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = exception
            };

            const int statusCode = StatusCodes.Status500InternalServerError;
            const string title = "Test title";
            const string link = "test.com";
            var details = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = link
            };
            var expectedResult = new ObjectResult(details)
            {
                StatusCode = statusCode
            };

            _options.ClientErrorMapping.Add(statusCode, new ClientErrorData
            {
                Title = title,
                Link = link
            });

            _classUnderTest.OnException(exceptionContext);

            _logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.Is<Exception>(e => e.IsSameOrEqualTo(exception)),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true))
            );
            exceptionContext.Result.Should().BeEquivalentTo(expectedResult);
        }
    }
}
