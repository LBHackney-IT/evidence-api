using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Filters;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.V1.Boundary.Request;

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

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userEmail = filterContext.HttpContext.Request.Headers["UserEmail"];
            if (userEmail.Count == 0)
            {
                filterContext.Result = new BadRequestObjectResult("UserEmail request header empty");
            }

            var userEmailValue = userEmail[0];

            if (userEmailValue == "resident-dummy-value")
            {
                base.OnActionExecuting(filterContext);
            }

            var path = filterContext.HttpContext.Request.Path.Value;
            var method = filterContext.HttpContext.Request.Method;
            var queryString = filterContext.HttpContext.Request.QueryString.Value;
            var auditEventRequest = new AuditEventRequest
            {
                Path = path,
                Method = method,
                QueryString = queryString,
                UserEmail = userEmailValue
            };
            _createAuditUseCase.Execute(auditEventRequest);
            // Pass the request onto the controller it was intended for
            base.OnActionExecuting(filterContext);
        }
    }
}
