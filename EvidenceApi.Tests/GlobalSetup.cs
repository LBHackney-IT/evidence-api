using System;
using System.Diagnostics.CodeAnalysis;
using dotenv.net;
using NUnit.Framework;
using System.IO;

[SetUpFixture]
public class GlobalSetup
{
    [OneTimeSetUp]
    [SuppressMessage("ReSharper", "CA1031")]
    public void SetUp()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        try
        {
            DotEnv.Load();
        }
        catch
        {
            Console.Write("Could not find .env file");
        }
    }
}
