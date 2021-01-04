using System;
using dotenv.net;
using NUnit.Framework;

namespace EvidenceApi.Tests
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var success = DotEnv.AutoConfig(5);
            Console.WriteLine($"ENV: {success}");
        }
    }
}
