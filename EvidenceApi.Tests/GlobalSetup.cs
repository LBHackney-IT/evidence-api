using System;
using dotenv.net;
using NUnit.Framework;
using System.IO;

namespace EvidenceApi.Tests
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            DotEnv.Config(true, Path.GetFullPath("../../../../.env.example"));
        }
    }
}
