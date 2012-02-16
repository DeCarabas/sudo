namespace sudo
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Pipes;
    using System.Reflection;
    using System.Security.Principal;
    using sudo.Protocol;

    static class Client
    {
        public static void Usage()
        {
            Console.WriteLine("Usage: sudo [command line to run as administrator]");
        }

        static void Elevate()
        {
            Process.Start(new ProcessStartInfo()
            {
                Arguments = "-Embedding",
                CreateNoWindow = true,
                FileName = Assembly.GetCallingAssembly().Location,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = Environment.CurrentDirectory
            });
        }

        public static NamedPipeClientStream TryConnect(TimeSpan timeout)
        {
            try
            {
                var client = new NamedPipeClientStream(
                    ".",
                    ProtocolConstants.PipeName,
                    PipeDirection.InOut,
                    PipeOptions.None,
                    TokenImpersonationLevel.Identification);
                client.Connect((int)timeout.TotalMilliseconds);

                return client;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        public static int Run()
        {
            var stream = TryConnect(TimeSpan.FromSeconds(1));
            if (stream == null)
            {
                Elevate();
                stream = TryConnect(TimeSpan.FromSeconds(30));
                if (stream == null)
                {
                    Console.WriteLine("[[ Unable to contact sudo server! ]]");
                    return -7;
                }
            }

            using (stream)
            {
                var writer = new StreamWriter(stream);
                var reader = new StreamReader(stream);

                var programName = Environment.GetCommandLineArgs()[0];
                var commandLine = Environment.CommandLine;
                var index = commandLine.IndexOf(programName) + programName.Length;
                if (commandLine[index] == '"') { index++; } // Slight chance
                commandLine = commandLine.Substring(index);

                var request = new ExecuteRequest()
                {
                    ClientPid = Process.GetCurrentProcess().Id,
                    CommandLine = commandLine,
                    WorkingDirectory = Environment.CurrentDirectory
                };

                foreach(DictionaryEntry kvp in Environment.GetEnvironmentVariables())
                {
                    request.Environment.Add((string)kvp.Key, (string)kvp.Value);
                }

                request.Write(writer);
                writer.Flush();
                stream.Flush();

                var response = Int32.Parse(reader.ReadLine());
                var message = reader.ReadLine();

                if (message != "OK") { Console.WriteLine("Internal Error: {0}", message); }
                return response;
            }
        }
    }
}
