using System.Runtime.InteropServices;

namespace Utils;

public class FileSystem
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, IntPtr dwProcessId);

    [Flags]
    private enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VmOperation = 0x00000008,
        VmRead = 0x00000010,
        VmWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        Synchronize = 0x00100000
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
        IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
        uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);

    [Flags]
    private enum DuplicateOptions : uint
    {
        DuplicateCloseSource = 0x00000001,
        DuplicateSameAccess = 0x00000002
    }

    public static bool CloseHandleEx(IntPtr pid, IntPtr handle)
    {
        var hProcess = OpenProcess(ProcessAccessFlags.DupHandle, false, pid);
        var success = DuplicateHandle(hProcess, handle, IntPtr.Zero, out _, 0, false,
            DuplicateOptions.DuplicateCloseSource);
        CloseHandle(hProcess);
        return success;
    }
}