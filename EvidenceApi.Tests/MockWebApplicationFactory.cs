using System.Data.Common;
using System.Security.Cryptography;
using EvidenceApi.V1.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notify.Interfaces;

namespace EvidenceApi.Tests
{
    public class MockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly DbConnection _connection;
        private readonly INotificationClient _mockNotificationClient;
        private readonly AppOptions _options;

        public MockWebApplicationFactory(DbConnection connection, INotificationClient mockNotificationClient, AppOptions options)
        {
            _connection = connection;
            _mockNotificationClient = mockNotificationClient;
            _options = options;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .UseStartup<Startup>();

            builder.ConfigureServices(services =>
            {
                var dbBuilder = new DbContextOptionsBuilder();
                dbBuilder.UseNpgsql(_connection);
                var context = new EvidenceContext(dbBuilder.Options);
                services.AddSingleton(context);

                var serviceProvider = services.BuildServiceProvider();
                var dbContext = serviceProvider.GetRequiredService<EvidenceContext>();

                dbContext.Database.Migrate();
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddTransient(x => _mockNotificationClient);
                services.AddTransient(x => SHA512.Create());
                services.AddSingleton(x => _options);
            });
        }
    }
}
