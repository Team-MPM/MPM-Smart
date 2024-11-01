using System.Reflection;

namespace ControllerTests;

using System.Diagnostics;

public static class TestHelper
{
    public static void RunTestInSeparateProcess(Action testMethod)
    {
        var methodInfo = testMethod.Method;
        var fullyQualifiedName = $"{methodInfo.DeclaringType!.FullName}.{methodInfo.Name}";
        
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"vstest \"{Assembly.GetExecutingAssembly().Location}\" --Tests:{fullyQualifiedName}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(processStartInfo);
        
        if (process == null)
        {
            throw new Exception("Failed to start process");
        }
        
        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();
    }
    
    public static void RunTestInSeparateProcess(Func<Task> testMethod) => 
        RunTestInSeparateProcess(() => testMethod().GetAwaiter().GetResult());
}