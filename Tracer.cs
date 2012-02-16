namespace sudo
{
    using System;
    using System.Diagnostics;

    static class Tracer
    {
        static readonly object lockObject = new object();
        const string LogName = "sudo";
        static EventLog eventLog;

        public static string Source { get; set; }

        public static void Start()
        {
            lock (lockObject)
            {
                if (eventLog == null)
                {
                    if (!EventLog.SourceExists(Source)) { EventLog.CreateEventSource(Source, LogName); }
                    eventLog = new EventLog { Log = LogName, Source = Source };

                    eventLog.WriteEntry(String.Format("{0} starting at {1}", Source, DateTime.Now));
                }
            }
        }

        public static void WriteError(Exception error)
        {
            Start();
            eventLog.WriteEntry("Error occured: " + error.ToString(), EventLogEntryType.Error);
        }

        public static void WriteLine(string message, params object[] args)
        {
            Start();
            eventLog.WriteEntry(String.Format(message, args));
        }

        public static void WriteWarning(string message, params object[] args)
        {
            Start();
            eventLog.WriteEntry(String.Format(message, args), EventLogEntryType.Warning);
        }
    }
}
