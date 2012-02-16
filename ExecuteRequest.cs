namespace sudo
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    class ExecuteRequest
    {
        public static ExecuteRequest Read(TextReader reader)
        {
            StringWriter writer = new StringWriter(); 

            writer.WriteLine("Reading request...");
            var request = new ExecuteRequest();

            var pidString = reader.ReadLine();
            if (pidString == null) { return null; }
            
            int pid;
            if (!Int32.TryParse(pidString, out pid)) { return null; }
            request.ClientPid = pid;
            writer.WriteLine("PID: {0}", request.ClientPid);


            request.WorkingDirectory = reader.ReadLine();
            if (request.WorkingDirectory == null) { return null; }
            writer.WriteLine("Working Directory: {0}", request.WorkingDirectory);
            
            var environmentVariable = reader.ReadLine();
            while (environmentVariable != String.Empty)
            {
                if (environmentVariable == null) { return null; }

                var environmentValue = reader.ReadLine();
                if (environmentValue == null) { return null; }
                
                writer.WriteLine("Set: {0} = {1}", environmentVariable, environmentValue);
                request.Environment.Add(environmentVariable, environmentValue);

                environmentVariable = reader.ReadLine();
            }

            request.CommandLine = reader.ReadLine();
            if (request.CommandLine == null) { return null; }
            writer.WriteLine("Command Line: {0}", request.CommandLine);

            writer.Flush();
            Tracer.WriteLine(writer.ToString());

            return request;
        }

        Dictionary<string, string> environment = new Dictionary<string, string>();

        public int ClientPid { get; set; }
        public string CommandLine { get; set; }
        public Dictionary<string, string> Environment { get { return this.environment; } }
        public string WorkingDirectory { get; set; }

        public void Write(TextWriter writer)
        {
            writer.WriteLine(ClientPid);
            writer.WriteLine(WorkingDirectory);
            foreach(var kvp in Environment)
            {
                writer.WriteLine(kvp.Key);
                writer.WriteLine(kvp.Value);
            }
            writer.WriteLine();
            writer.WriteLine(CommandLine);
        }
    }
}
