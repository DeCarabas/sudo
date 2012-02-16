namespace sudo
{
    using System;
    using System.IO;

    // This class handles executing the actual process, after attaching the console appropriately.
    //
    public static class Executor
    {
        public const string MagicDelimiter = "///\\\\\\///";

        static void AttachConsole(int pid)
        {
            NativeMethods.FreeConsole();
            NativeMethods.AttachConsole(pid);

            var input = new StreamReader(NativeMethods.CreateFile(
                "CONIN$", FileAccess.Read, FileShare.ReadWrite, FileMode.Open, FileAttributes.Normal));
            
            var output = new FlushingWriter 
            {
                BaseWriter = new StreamWriter(
                    NativeMethods.CreateFile(
                        "CONOUT$", FileAccess.Write, FileShare.ReadWrite, FileMode.Open, FileAttributes.Normal))
            };

            Console.SetIn(input);
            Console.SetOut(output);
            Console.SetError(output);
        }

        // TODO: Understand a shell other than cmd.exe.
        //
        public static int Run(string[] args)
        {
            int pid = Int32.Parse(args[1]);

            Tracer.WriteLine("Freeing...");

            // Attach to the console of our parent.
            // We never redirected stdin/stdout/stderr so this should all be just fine.
            //
            AttachConsole(pid);

            Tracer.WriteLine("Attached...");

            // We know that the command line for the process looks like this:
            //  
            //   sudo.exe -Exec {pid} {Magic Delimiter} {rest of command line}
            //
            // We use the Magic Delimiter because it's highly unlikely to be in a real person's path. We want to avoid 
            // any parsing at all because it is very very complicated to get right. Therefore, we just split at 
            // the magic delimiter and call it good.
            //
            string commandToExecute = Environment.CommandLine.Split(
                new string[] { MagicDelimiter },
                2,
                StringSplitOptions.None)[1];

            string shellPath = Path.Combine(Environment.SystemDirectory, "cmd.exe");
            string commandLine = shellPath + " /C " + commandToExecute;

            // For some reason, I could not convince System.Diagnostics to behave correctly here, so I need to continue
            // the P/Invoke method all the way down.
            //
            // NOTE: This code is not "safe" at all. Fortunately we're in a throw-away process, so it doesn't really
            //       matter whether or not we leak handles.
            //            
            Console.WriteLine("[[ Running: {0} ]]", commandToExecute.Trim());
            Tracer.WriteLine("Running process [[{0}]]", commandLine);
            var pi = NativeMethods.CreateProcess(
                shellPath,
                commandLine,
                IntPtr.Zero,
                IntPtr.Zero,
                true,
                0,
                IntPtr.Zero,
                null);

            NativeMethods.WaitForSingleObject(pi.hProcess, -1);

            int exitCode = NativeMethods.GetExitCodeProcess(pi.hProcess);
            Tracer.WriteLine("Finished: {0}", exitCode);
            return exitCode;
        }
    }
}
