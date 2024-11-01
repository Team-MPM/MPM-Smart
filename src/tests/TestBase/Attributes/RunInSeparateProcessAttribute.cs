// using NUnit.Framework.Interfaces;
// using NUnit.Framework.Internal;
// using NUnit.Framework.Internal.Commands;
//
// namespace ControllerTests;
//
// [AttributeUsage(AttributeTargets.Method)]
// public class RunInSeparateProcessAttribute : NUnitAttribute, IWrapSetUpTearDown
// {
//     public TestCommand Wrap(TestCommand command) => new SeparateProcessCommand(command);
// }
//
// public class SeparateProcessCommand(TestCommand innerCommand) : TestCommand(innerCommand.Test)
// {
//     public override TestResult Execute(TestExecutionContext context)
//     {
//         var process = Process.Start("nunit3-console.exe", $"{Test.FullName}.dll");
//         process.WaitForExit();
//         return context.CurrentResult;
//     }
// }