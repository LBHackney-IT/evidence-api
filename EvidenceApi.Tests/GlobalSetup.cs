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
        try
        {
            DotEnv.Load();
            //DotEnv.Config(true, Path.GetFullPath("../../../../.env"));
        }
        catch
        {
            Console.Write("Could not find .env file");
        }
    }
}
