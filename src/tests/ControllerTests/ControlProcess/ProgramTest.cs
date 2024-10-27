﻿using System.Diagnostics;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using ControlProcess;
using JetBrains.Annotations;
using Xunit;

namespace ControllerTests.ControlProcess;

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
    public async Task LaunchControlProcess_ShouldMonitorProcess()
    {
        var restartCallbackCallCount = 0;

        Program.LaunchControlProcess(m_Logger, BackendExecutablePath, BackendProcessName,
            _ => { restartCallbackCallCount++; });

        var client = m_HttpClientFactory.CreateClient(HttpClientFactoryHelper.TestClientName);

        var response1 = await client.GetAsync("/kys");
        await Task.Delay(500);
        var response2 = await client.GetAsync("/kys");
        await Task.Delay(500);

        Assert.Multiple(() =>
        {
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            Assert.Equal(2, restartCallbackCallCount);
        });
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