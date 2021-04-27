using EvidenceApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EvidenceApi.V1.Controllers
{
    public class BaseController : Controller
    {
        private readonly ICreateAuditUseCase _createAuditUseCase;

        public BaseController(ICreateAuditUseCase createAuditUseCase)
        {
            _createAuditUseCase = createAuditUseCase;
            ConfigureJsonSerializer();
        }

        public static void ConfigureJsonSerializer()
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;

                return settings;
            };
        }

        /**
         * Description about what this does
         * Including why we added it here instead of adding it as a GlobalFilter
         * Create an ADR
         */
        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            var hackneyToken = filterContext.HttpContext.Request.Headers["HackneyToken"];
            if (hackneyToken.Count == 0)
            {
                filterContext.Result = new BadRequestObjectResult("HackneyToken request header empty");
            }
            else
            {
                var path = filterContext.HttpContext.Request.Path.Value;
                var hackneyTokenValue = hackneyToken[0];
                _createAuditUseCase.Execute(path, hackneyTokenValue);
                // Pass the request onto the controller it was intended for
                base.OnActionExecuting(filterContext);
            }
        }
    }
}
