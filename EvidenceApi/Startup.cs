using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using dotenv.net;
using EvidenceApi.V1.Boundary.Request;
using EvidenceApi.V1.Controllers;
using EvidenceApi.V1.Controllers.Filters;
using EvidenceApi.V1.Domain;
using EvidenceApi.V1.Gateways;
using EvidenceApi.V1.Gateways.Interfaces;
using EvidenceApi.V1.Infrastructure;
using EvidenceApi.V1.Infrastructure.Interfaces;
using EvidenceApi.V1.UseCase;
using EvidenceApi.V1.UseCase.Interfaces;
using EvidenceApi.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Notify.Client;
using Notify.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EvidenceApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            AWSSDKHandler.RegisterXRayForAllServices();
        }

        public IConfiguration Configuration { get; }
        private const string ApiName = "Evidence API";

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc(c =>
                {
                    c.Filters.Add<HandleExceptionAttribute>();
                });

            var apiVersions = new List<ApiVersion>();
            apiVersions.Add(new ApiVersion(1, 0));

            foreach (var apiVersion in apiVersions)
            {
                services.AddApiVersioning(o =>
                {
                    o.DefaultApiVersion = apiVersion;
                    o.AssumeDefaultVersionWhenUnspecified =
                        true; // assume that the caller wants the default version if they don't specify
                    o.ApiVersionReader =
                        new UrlSegmentApiVersionReader(); // read the version number from the url segment header)
                });
            }

            services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Token",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Your Hackney API Key",
                        Name = "X-Api-Key",
                        Type = SecuritySchemeType.ApiKey
                    });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Token" }
                        },
                        new List<string>()
                    }
                });

                c.OperationFilter<UserTokenHeaderParameterOperationFilter>();

                //Looks at the APIVersionAttribute [ApiVersion("x")] on controllers and decides whether or not
                //to include it in that version of the swagger document
                //Controllers must have this [ApiVersion("x")] to be included in swagger documentation!!
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    apiDesc.TryGetMethodInfo(out var methodInfo);

                    var versions = methodInfo?
                        .DeclaringType?.GetCustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions).ToList();

                    return versions?.Any(v => $"{v.GetFormattedApiVersion()}" == docName) ?? false;
                });

                //Get every ApiVersion attribute specified and create swagger docs for them
                foreach (var apiVersion in apiVersions)
                {
                    var version = $"v{apiVersion.ToString()}";
                    c.SwaggerDoc(version,
                        new OpenApiInfo
                        {
                            Title = $"{ApiName}-api {version}",
                            Version = version,
                            Description =
                                $"{ApiName} version {version}. Please check older versions for deprecated endpoints."
                        });
                }

                c.CustomSchemaIds(x => x.FullName);
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });

            DotEnv.Fluent().WithEnvFiles(Path.Combine(Directory.GetCurrentDirectory(), "../.env")).Load();

            var options = AppOptions.FromEnv();
            services.AddSingleton(x => options);

            // Database Context
            services.AddDbContext<EvidenceContext>(
                opt => opt.UseNpgsql(options.DatabaseConnectionString));

            // Transients
            services.AddTransient<INotificationClient>(x => new NotificationClient(options.NotifyApiKey));

            // Gateways
            services.AddScoped<IDocumentTypeGateway, DocumentTypeGateway>(sp =>
                new DocumentTypeGateway(new FileReader<List<Team>>(options.DocumentTypeConfigPath)));
            services.AddScoped<IStaffSelectedDocumentTypeGateway, StaffSelectedDocumentTypeGateway>(sp =>
                new StaffSelectedDocumentTypeGateway(new FileReader<List<Team>>(options.StaffSelectedDocumentTypeConfigPath)));
            services.AddScoped<IResidentsGateway, ResidentsGateway>();
            services.AddScoped<IEvidenceGateway, EvidenceGateway>();
            services.AddScoped<INotifyGateway, NotifyGateway>();
            services.AddScoped<IStringHasher, StringHasher>();
            services.AddHttpClient<IDocumentsApiGateway, DocumentsApiGateway>();

            // Use Cases
            services.AddScoped<ICreateEvidenceRequestUseCase, CreateEvidenceRequestUseCase>();
            services.AddScoped<IValidator<ResidentRequest>, ResidentRequestValidator>();
            services.AddScoped<IEvidenceRequestValidator, EvidenceRequestValidator>();
            services.AddScoped<IFindEvidenceRequestByIdUseCase, FindEvidenceRequestByIdUseCase>();
            services.AddScoped<ICreateDocumentSubmissionUseCase, CreateDocumentSubmissionUseCase>();
            services.AddScoped<IUpdateDocumentSubmissionStateUseCase, UpdateDocumentSubmissionStateUseCase>();
            services.AddScoped<IFindEvidenceRequestsUseCase, FindEvidenceRequestsUseCase>();
            services.AddScoped<IFindResidentByIdUseCase, FindResidentByIdUseCase>();
            services.AddScoped<IFindDocumentSubmissionByIdUseCase, FindDocumentSubmissionByIdUseCase>();
            services.AddScoped<IUpdateEvidenceRequestStateUseCase, UpdateEvidenceRequestStateUseCase>();
            services.AddScoped<IFindResidentsBySearchQueryUseCase, FindResidentsBySearchQueryUseCase>();
            services.AddScoped<IFindDocumentSubmissionsByResidentIdUseCase, FindDocumentSubmissionsByResidentIdUseCase>();
            services.AddScoped<IFindOrCreateResidentReferenceIdUseCase, FindOrCreateResidentReferenceIdUseCase>();
            services.AddScoped<ICreateAuditUseCase, CreateAuditUseCase>();
            services.AddScoped<ISendNotificationUploadConfirmationToResidentAndStaff, SendNotificationUploadConfirmationToResidentAndStaff>();
            services.AddScoped<IGetDocumentTypesByTeamNameUseCase, GetDocumentTypesByTeamNameUseCase>();
            services
                .AddScoped<IGetStaffSelectedDocumentTypesByTeamNameUseCase,
                    GetStaffSelectedDocumentTypesByTeamNameUseCase>();
            services.AddScoped<ICreateDocumentSubmissionWithoutEvidenceRequestUseCase, CreateDocumentSubmissionWithoutEvidenceRequestUseCase>();
            services.AddScoped<ICreateResidentUseCase, CreateResidentUseCase>();
            services.AddScoped<IGetAllGroupIdsByResidentIdsUseCase, GetAllGroupIdsByResidentIdsUseCase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //Get All ApiVersions,
            var api = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
            var swaggerUiApiVersions = api.ApiVersionDescriptions.ToList();

            //Swagger ui to view the swagger.json file
            app.UseSwaggerUI(c =>
            {
                foreach (var apiVersionDescription in swaggerUiApiVersions)
                {
                    //Create a swagger endpoint for each swagger version
                    c.SwaggerEndpoint($"{apiVersionDescription.GetFormattedApiVersion()}/swagger.json",
                        $"{ApiName}-api {apiVersionDescription.GetFormattedApiVersion()}");
                }
            });

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            app.UseSwagger();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // SwaggerGen won't find controllers that are routed via this technique.
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
