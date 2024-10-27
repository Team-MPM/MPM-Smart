using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using ControlProcess;

namespace ControllerTests.ControlProcess;

[TestFixture]
[TestOf(typeof(Program))]
public class ProgramTest
{
    private const string BackendExecutablePath = "../backend/backend.exe";
    private const string BackendProcessName = "Mpm-Smart-Backend-Test-Instance";

    private ILogger m_Logger;
    private IHttpClientFactory m_HttpClientFactory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Directory.SetCurrentDirectory("../../../../../../build/controller/control-process");
        m_HttpClientFactory = HttpClientFactoryHelper.CreateHttpClientFactory("http://localhost:54321");
    }

    [SetUp]
    public void SetUp()
    {
        m_Logger = new Mock<ILogger>().Object;
    }

    [Test]
    public void LaunchControlProcess_ShouldLaunchControlProcess()
    {
        var result = Program.LaunchControlProcess(m_Logger, BackendExecutablePath, BackendProcessName, _ => { });
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task LaunchControlProcess_ShouldMonitorProcess()
    {
        var restartCallbackCallCount = 0;

        Program.LaunchControlProcess(m_Logger, BackendExecutablePath, BackendProcessName, _ =>
        {
            restartCallbackCallCount++;
        });

        var client = m_HttpClientFactory.CreateClient(HttpClientFactoryHelper.TestClientName);

        var response1 = await client.GetAsync("/kys");
        await Task.Delay(500);
        var response2 = await client.GetAsync("/kys");
        await Task.Delay(500);

        Assert.Multiple(() =>
        {
            Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(restartCallbackCallCount, Is.GreaterThan(1));
        });
    }

    [Test]
    public void LaunchControlProcess_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            Program.LaunchControlProcess(m_Logger, BackendExecutablePath, BackendProcessName, _ => { });
        });
    }
    
    [Test]
    public void CreateControlLogger_ShouldCreateControlLogger()
    {
        var result = Program.CreateControlLogger();
        Assert.That(result, Is.Not.Null);
    }
}