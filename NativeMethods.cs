namespace sudo
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using Microsoft.Win32.SafeHandles;

    [SecurityCritical]
    [SuppressUnmanagedCodeSecurity]
    static class NativeMethods
    {
        const int GENERIC_READ = unchecked((int)0x80000000);
        const int GENERIC_WRITE = unchecked((int)0x40000000);

        public static void AttachConsole(int dwProcessId)
        {
            if (!_AttachConsole(dwProcessId))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        public static Stream CreateFile(
            string fileName, FileAccess fileAccess, FileShare fileShare, FileMode fileMode, FileAttributes flags)
        {
            // TODO: This is not quite right, but it's close.
            //
            var nativeAccess = fileAccess;
            if ((nativeAccess & FileAccess.Read) != 0)
            {
                nativeAccess &= ~FileAccess.Read;
                nativeAccess |= (FileAccess)GENERIC_READ;
            }
            if ((nativeAccess & FileAccess.Write) != 0)
            {
                nativeAccess &= ~FileAccess.Write;
                nativeAccess |= (FileAccess)GENERIC_WRITE;
            }

            var handle = _CreateFile(fileName, nativeAccess, fileShare, IntPtr.Zero, fileMode, flags, IntPtr.Zero);
            if (handle.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            return new SimpleFileStream(handle);
        }

        public static PROCESS_INFORMATION CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory)
        {
            var startupInfo = new STARTUPINFO();
            startupInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));

            PROCESS_INFORMATION pi;
            if (!_CreateProcess(
                lpApplicationName,
                lpCommandLine,
                lpProcessAttributes,
                lpThreadAttributes,
                bInheritHandles,
                dwCreationFlags,
                lpEnvironment,
                lpCurrentDirectory,
                ref startupInfo,
                out pi))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            return pi;
        }

        public static void FreeConsole()
        {
            if (!_FreeConsole())
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
        }

        public static int GetExitCodeProcess(IntPtr hProcess)
        {
            int ret;
            if (!_GetExitCodeProcess(hProcess, out ret))
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            return ret;
        }

        public static int ReadFile(SafeFileHandle file, byte[] buffer, int offset, int count)
        {
            if (offset >= buffer.Length) { throw new ArgumentOutOfRangeException("offset"); }
            if (count > buffer.Length - offset) { throw new ArgumentOutOfRangeException("count"); }

            unsafe
            {
                fixed (byte* bufferStart = buffer)
                {
                    int read = 0;
                    byte* bufferPointer = bufferStart + offset;
                    if (!_ReadFile(file, bufferPointer, count, out read, IntPtr.Zero))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }

                    return read;
                }
            }
        }

        public static int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds)
        {
            int ret = _WaitForSingleObject(hHandle, dwMilliseconds);
            if (ret == -1)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            return ret;
        }

        public static int WriteFile(SafeFileHandle file, byte[] buffer, int offset, int count)
        {
            if (offset >= buffer.Length) { throw new ArgumentOutOfRangeException("offset"); }
            if (count >= buffer.Length - offset) { throw new ArgumentOutOfRangeException("count"); }

            unsafe
            {
                fixed (byte* bufferStart = buffer)
                {
                    int wrote = 0;
                    byte* bufferPointer = bufferStart + offset;
                    if (!_WriteFile(file, bufferPointer, count, out wrote, IntPtr.Zero))
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }

                    return wrote;
                }
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "AttachConsole", SetLastError = true)]
        static extern bool _AttachConsole(int dwProcessId);

        [DllImport("Kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern SafeFileHandle _CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flags,
            IntPtr template);

        [DllImport("kernel32.dll", EntryPoint = "CreateProcessW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool _CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", EntryPoint = "FreeConsole", SetLastError = true)]
        static extern bool _FreeConsole();

        [DllImport("kernel32.dll", EntryPoint = "GetExitCodeProcess", SetLastError = true)]
        static extern bool _GetExitCodeProcess(IntPtr hProcess, out int lpExitCode);

        [DllImport("kernel32.dll", EntryPoint = "ReadFile", SetLastError = true)]
        static unsafe extern bool _ReadFile(
            SafeFileHandle hFile,
            byte* lpBuffer,
            int nNumberOfBytesToRead,
            out int lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject", SetLastError = true)]
        static extern int _WaitForSingleObject([In] IntPtr hHandle, [In] int dwMilliseconds);

        [DllImport("kernel32.dll", EntryPoint = "WriteFile", SetLastError = true)]
        static unsafe extern bool _WriteFile(
            SafeFileHandle hFile,
            byte* lpBuffer,
            int nNumberOfBytesToWrite,
            out int lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
    }
}
