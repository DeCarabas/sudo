namespace sudo.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    sealed class ExecuteResponse
    {
        public int ExitCode { get; set; }

        public static ExecuteResponse Read( TextReader reader )
        {
            var response = new ExecuteResponse();

            response.ExitCode = Int32.Parse(reader.ReadLine());

            return response;
        }

        public void Write( TextWriter writer )
        {
            writer.WriteLine( ExitCode );
        }
    }
}
