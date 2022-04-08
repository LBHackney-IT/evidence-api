using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EvidenceApi.V1.Controllers.Filters
{
    [UsedImplicitly]
    public class HandleExceptionAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<HandleExceptionAttribute> _logger;
        private readonly ApiBehaviorOptions _options;

        public HandleExceptionAttribute(
            ILogger<HandleExceptionAttribute> logger,
            IOptions<ApiBehaviorOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, context.Exception.Message);

            const int statusCode = StatusCodes.Status500InternalServerError;
            var clientErrorData = _options.ClientErrorMapping[statusCode];
            var details = new ProblemDetails
            {
                Status = statusCode,
                Title = clientErrorData.Title,
                Type = clientErrorData.Link
            };

            context.Result = new ObjectResult(details)
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;

            base.OnException(context);
        }
    }
}
