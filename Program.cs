namespace sudo
{
    using System;

    static class Program
    {
        // There are three parts of this program.
        //
        //  (1) The command line Client that users invoke, which communicates with and potentially starts
        //  (2) The named pipe Server which is executing as administrator, which executes instances of
        //  (3) The Executor which attaches to the client's console and executes the command.
        //
        // We determine which is which by examining the command line arguments.
        //
        static void Main(string[] args)
        {
            bool reportErrors = true;
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { Tracer.WriteError(e.ExceptionObject as Exception); };

            try
            {
                if (args.Length == 0)
                {
                    // Must be command line, but no actual arguments
                    //
                    Tracer.Source = "sudo client";
                    Client.Usage();
                }
                else if (args[0] == "-Embedding")
                {
                    // Being run as a named pipes server
                    //
                    Tracer.Source = "sudo server";
                    reportErrors = false;                    
                    Server.Run();
                }
                else if (args[0] == "-Exec")
                {
                    // Being run to execute the child process
                    //
                    Tracer.Source = "sudo runner";
                    Environment.Exit(Executor.Run(args));
                }
                else
                {
                    // Command line client
                    //
                    Tracer.Source = "sudo client";
                    Environment.Exit(Client.Run());
                }
            }
            catch (Exception e)
            {
                if (reportErrors)
                {
                    Console.Error.WriteLine("Error occured: {0}", e);
                    Console.ReadLine();
                    Environment.Exit(-1);
                }
                
                throw;
            }
        }
    }
}
