using System.Diagnostics;

const string processPath = @"../backend/backend.exe";
const string processName = "Mpm-Smart-Backend";

Console.WriteLine($"[INFO] Process Controller for {processName} started.");

try
{
    while (true)
        StartAndMonitorProcess();
}
catch (Exception e)
{
    Console.WriteLine(e);
}
finally
{
    Console.WriteLine($"[INFO] Process Controller for {processName} stopped.");
}

return 1;

void StartAndMonitorProcess()
{
    var process = new Process();

    process.StartInfo.FileName = processPath;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.EnableRaisingEvents = true;

    process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine($"[OUTPUT] {e.Data}"); };
    process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine($"[ERROR] {e.Data}"); };

    Console.WriteLine($"[INFO] Starting process: {processPath}");
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    Console.WriteLine($"[INFO] Process {process.Id} started.");

    process.WaitForExit();

    Console.WriteLine($"[INFO] Process {process.Id} exited with code {process.ExitCode}.");

    ReloadPlugins();

    Console.WriteLine("[INFO] Restarting process...");
}

static void ReloadPlugins()
{
    Console.WriteLine("[ACTION] Performing plugin reload...");

    // TODO
}