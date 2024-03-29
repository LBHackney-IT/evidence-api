using System.Data;
using System.Data.Common;
using System.Net.Http;
using AutoFixture;
using AutoFixture.AutoMoq;
using EvidenceApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Notify.Interfaces;
using Npgsql;
using NUnit.Framework;
using WireMock.Server;

namespace EvidenceApi.Tests
{
    public class IntegrationTests<TStartup> where TStartup : class
    {
        protected HttpClient Client { get; private set; }
        protected EvidenceContext DatabaseContext { get; private set; }
        protected Mock<INotificationClient> MockNotifyClient { get; private set; }
        protected WireMockServer DocumentsApiServer { get; private set; }
        protected AppOptions Options { get; private set; }

        private MockWebApplicationFactory<TStartup> _factory;
        private NpgsqlConnection _connection;
        private DbTransaction _transaction;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Options = new Fixture().Create<AppOptions>();
            Options.DocumentsApiUrl = new System.Uri("http://localhost:3103");

            _connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            _connection.Open();
            var npgsqlCommand = _connection.CreateCommand();
            npgsqlCommand.CommandText = "SET deadlock_timeout TO 30";
            npgsqlCommand.ExecuteNonQuery();
            DocumentsApiServer = WireMockServer.Start(3103);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DocumentsApiServer.Stop();
        }

        [SetUp]
        public void BaseSetup()
        {
            MockNotifyClient = CreateMockNotifyClient();
            _factory = new MockWebApplicationFactory<TStartup>(_connection, MockNotifyClient.Object, Options);
            Client = _factory.CreateClient();
            // setting value for UserEmail HTTP header so BaseController passes the request to the controller
            Client.DefaultRequestHeaders.Add("UserEmail", "resident-dummy-value");
            DatabaseContext = _factory.Server.Host.Services.GetRequiredService<EvidenceContext>();

            _transaction = _connection.BeginTransaction(IsolationLevel.RepeatableRead);
            DatabaseContext.Database.UseTransaction(_transaction);
        }

        [TearDown]
        public void BaseTearDown()
        {
            Client.Dispose();
            _factory.Dispose();
            _transaction.Rollback();
            _transaction.Dispose();
        }

        private static Mock<INotificationClient> CreateMockNotifyClient()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization());
            var mockClient = fixture.Freeze<Mock<INotificationClient>>();
            fixture.Create<INotificationClient>();
            return mockClient;
        }
    }
}
