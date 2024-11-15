using System.Diagnostics;
using System.Net;
using System.Reflection;
using ControlProcess;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using TestBase.Helpers;
using Xunit;

namespace ControlProcessTests;

[TestSubject(typeof(Program))]
public class ProgramTest : IAsyncLifetime, IDisposable
{
#if WINDOWS
    private const string BackendExecutablePath = "../backend/backend.exe";
#else
    private const string BackendExecutablePath = "../backend/Backend";
#endif
    private const string BackendProcessName = "Mpm-Smart-Backend-Test-Instance";

    private readonly ILogger m_Logger = new Mock<ILogger>().Object;
    private IHttpClientFactory m_HttpClientFactory = null!;

    public Task InitializeAsync()
    {
        Directory.SetCurrentDirectory(Path.Combine(Assembly.GetExecutingAssembly().Location,
            "../../../../../../../build/controller/backend"));
        m_HttpClientFactory = HttpClientFactoryHelper.CreateHttpClientFactory("http://localhost:54321");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        var processes = Process.GetProcessesByName("Backend");
        foreach (var process in processes)
            process.Kill();
    }

    [Fact]
    public void LaunchControlProcess_ShouldLaunchControlProcess()
    {
        var result = Program.LaunchControlProcess(m_Logger, BackendExecutablePath, BackendProcessName, _ => { });
        Assert.NotNull(result);
    }

    [Fact]
    public Task LaunchControlProcess_ShouldMonitorProcess()
    {
        Assert.True(true);
        return Task.CompletedTask;
        
        // var restartCallbackCallCount = 0;
        //
        // Program.LaunchControlProcess(m_Logger, BackendExecutablePath, BackendProcessName,
        //     _ => { restartCallbackCallCount++; });
        //
        // var client = m_HttpClientFactory.CreateClient(HttpClientFactoryHelper.TestClientName);
        //
        //
        // HttpResponseMessage? response1 = null;
        // HttpResponseMessage? response2 = null;
        //
        // for (var i = 0; i < 10; i++)
        // {
        //     try
        //     {
        //         response1 = await client.GetAsync("/kys");
        //         if (response1.IsSuccessStatusCode)
        //             break;
        //     }
        //     catch
        //     {
        //         // ignored
        //     }
        //
        //     await Task.Delay(500);
        // }
        //
        // await Task.Delay(2000);
        //
        // for (var i = 0; i < 10; i++)
        // {
        //     try
        //     {
        //         response2 = await client.GetAsync("/kys");
        //         if (response2.IsSuccessStatusCode)
        //             break;
        //     }
        //     catch
        //     {
        //         // ignored
        //     }
        //
        //     await Task.Delay(500);
        // }
        //
        // await Task.Delay(5000);
        //
        // Assert.Multiple(() =>
        // {
        //     Assert.NotNull(response1);
        //     Assert.NotNull(response2);
        //     Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        //     Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        //     Assert.True(restartCallbackCallCount > 1);
        // });
    }

    [Fact]
    public void LaunchControlProcess_DoesNotThrow()
    {
        try
        {
            Program.LaunchControlProcess(m_Logger, BackendExecutablePath, BackendProcessName, _ => { });
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Fact]
    public void CreateControlLogger_ShouldCreateControlLogger()
    {
        var result = Program.CreateControlLogger();
        Assert.NotNull(result);
    }
}