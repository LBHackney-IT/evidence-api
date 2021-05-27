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
            if (SkipAuditForResidentRequest(userEmailValue))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var path = filterContext.HttpContext.Request.Path.Value;
            var method = filterContext.HttpContext.Request.Method;
            var request = "";

            if (method == "POST" || method == "PATCH")
            {
                request = JsonConvert.SerializeObject(filterContext.ActionArguments["request"], Formatting.None);
            }

            var queryString = filterContext.HttpContext.Request.QueryString.Value;

            if (method == "GET")
            {
                request = queryString;
            }

            var auditEventRequest = new AuditEventRequest
            {
                Path = path,
                Method = method,
                Request = request,
                UserEmail = userEmailValue
            };

            _createAuditUseCase.Execute(auditEventRequest);
            // Pass the request onto the controller it was intended for
            base.OnActionExecuting(filterContext);
        }

        private static bool SkipAuditForResidentRequest(string userEmailValue)
        {
            return userEmailValue == "resident-dummy-value";
        }
    }
}
