using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;

namespace ControlProcess;

internal class Program
{
    public const string BackendExecutablePath = "../backend/backend.exe";
    public const string BackendProcessName = "Mpm-Smart-Backend";
    public const int SignalWatchdogStopCode = 100;

    public static void Main(string[] args)
    {
        var logger = CreateControlLogger();

        var backendProcess = LaunchControlProcess(logger, BackendExecutablePath, BackendProcessName, ReloadPlugins);

        backendProcess.Join();
    }

    public static void StartAndMonitorProcess(ILogger logger, string path, Action<ILogger> restartCallback)
    {
        while (true)
        {
            var process = new Process();

            process.StartInfo.FileName = path;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) logger.LogTrace("{Message}", e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) logger.LogError("{Message}", e.Data);
            };

            logger.LogInformation($"Starting process: {BackendExecutablePath}");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            logger.LogInformation("Process {ProcessId} started", process.Id);

            process.WaitForExit();

            logger.LogInformation("Process {ProcessId} exited with code {ProcessExitCode}", process.Id,
                process.ExitCode);

            if (process.ExitCode == SignalWatchdogStopCode)
            {
                logger.LogInformation("Process {ProcessId} signaled watchdog to stop", process.Id);
                break;
            }

            restartCallback(logger);

            logger.LogInformation(" Restarting process...");
        }
    }

    public static void ReloadPlugins(ILogger logger)
    {
        logger.LogInformation("Performing plugin reload...");

        // TODO
    }

    public static ILogger CreateControlLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            var config = new LoggingConfiguration();

            config.AddRuleForAllLevels(new ConsoleTarget("console")
            {
                Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}",
                Header = "Control Process",
                Footer = "End of Control Process"
            });

            config.AddRuleForAllLevels(new FileTarget("file")
            {
                FileName = "control-process.log",
                Layout = "${longdate} ${logger} ${message}",
                Header = "Control Process",
                Footer = "End of Control Process"
            });

            builder.AddNLog(config);
        });

        return loggerFactory.CreateLogger("Mpm-Smart-Watchdog");
    }

    public static Thread LaunchControlProcess(ILogger logger, string path, string processName, Action<ILogger> restartCallback)
    {
        try
        {
            var t = new Thread(() => StartAndMonitorProcess(logger, path, restartCallback))
            {
                IsBackground = true,
                Name = processName,
                Priority = ThreadPriority.AboveNormal
            };

            t.Start();

            logger.LogInformation("Process Controller for {ProcessName} started", processName);

            return t;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to start process controller for {ProcessName}", processName);
            throw;
        }
    }
}