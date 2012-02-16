namespace sudo
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Pipes;
    using System.Reflection;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Threading;

    public class Coordinator
    {
        const int CommandLineLength = 4096;         
        static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(5);
        static readonly TimerCallback IdleCallback = new TimerCallback(OnIdleCallback);

        int activeConnections;
        CoordinatorState currentState;
        readonly Timer idleTimer;

        public Coordinator()
        {
            this.idleTimer = new Timer(IdleCallback, this, Timeout.Infinite, Timeout.Infinite);
        }

        public static string PipeName
        {
            get { return "Sudo-Commands-For-" + WindowsIdentity.GetCurrent().User.ToString(); }
        }

        void ChangeState(CoordinatorState coordinatorState)
        {
            throw new NotImplementedException();
        }

        void IdleTimerFired()
        {
            throw new NotImplementedException();
        }

        static Connection ListenForConnection()
        {
            var user = WindowsIdentity.GetCurrent().User;
            var security = new PipeSecurity();
            security.AddAccessRule(new PipeAccessRule(user, PipeAccessRights.FullControl, AccessControlType.Allow));
            security.SetOwner(user);
            security.SetGroup(user);

            var serverStream = new NamedPipeServerStream(
                PipeName,
                PipeDirection.InOut,
                20,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                CommandLineLength,
                CommandLineLength,
                security);

            serverStream.BeginWaitForConnection(OnConnectionCallback, this);

            throw new NotImplementedException();
        }

        static void OnConnectionCallback(IAsyncResult result)
        {

        }

        static void OnIdleCallback(object state)
        {
            ((Coordinator)state).IdleTimerFired();
        }

        void SetIdleTimer()
        {
            this.idleTimer.Change(IdleTimeout, TimeSpan.FromMilliseconds(-1));
        }

        void Shutdown()
        {
            throw new NotImplementedException();
        }

        abstract class CoordinatorState
        {
            public virtual void EnterState(Coordinator server) { }

            public abstract void ReceivedConnection(Coordinator server, IAsyncResult result);
            public abstract void ConnectionClosed(Coordinator server, Connection connection);
            public abstract void IdleTimerFired(Coordinator server);
        }

        class Idle : CoordinatorState
        {
            public static readonly CoordinatorState Instance = new Idle();

            private Idle() { }

            public override void EnterState(Coordinator server)
            {
                server.SetIdleTimer();
            }

            public override void ReceivedConnection(Coordinator server, IAsyncResult result)
            {
                //throw new NotImplementedException();

                server.ChangeState(Active.Instance);
            }

            public override void ConnectionClosed(Coordinator server, Connection connection)
            {
                throw new NotImplementedException();
            }

            public override void IdleTimerFired(Coordinator server)
            {
                server.ChangeState(ShuttingDown.Instance);
            }
        }

        class Active : CoordinatorState
        {
            public static readonly CoordinatorState Instance = new Active();

            private Active() { }

            public override void ReceivedConnection(Coordinator server, IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public override void ConnectionClosed(Coordinator server, Connection connection)
            {
                throw new NotImplementedException();
            }

            public override void IdleTimerFired(Coordinator server)
            {
                throw new NotImplementedException();
            }
        }

        class ShuttingDown : CoordinatorState
        {
            public static readonly CoordinatorState Instance = new ShuttingDown();

            private ShuttingDown() { }

            public override void ReceivedConnection(Coordinator server, IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public override void ConnectionClosed(Coordinator server, Connection connection)
            {
                throw new NotImplementedException();
            }

            public override void IdleTimerFired(Coordinator server)
            {
                throw new NotImplementedException();
            }
        }

        class Finished : CoordinatorState
        {
            public static readonly CoordinatorState Instance = new Finished();

            private Finished();

            public override void EnterState(Coordinator server)
            {
                server.Shutdown();
            }

            public override void ReceivedConnection(Coordinator server, IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public override void ConnectionClosed(Coordinator server, Connection connection)
            {
                throw new NotImplementedException();
            }

            public override void IdleTimerFired(Coordinator server)
            {
                throw new NotImplementedException();
            }
        }





    }


    class Connection
    {
        readonly NamedPipeServerStream listener;


    }


    // This class has both a static and non-static aspect. The static aspect handles running the COM server and 
    // managing idle timeout. The instance aspect handles individual elevation requests. Remember that the entire 
    // point is that the server remains running for a while, so that sudo doesn't need to re-elevate.
    //
    public class Server
    {
        const int CommandLineLength = 4096;

        static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(5);

        static int ActiveServers = 0;
        static Timer IdleTimer;
        static NamedPipeServerStream Listener;

        static object ServerLock = new object();
        static ManualResetEvent NoActiveServers = new ManualResetEvent(true);
        static ManualResetEvent ServersIdle = new ManualResetEvent(false);

        static Server()
        {
            IdleTimer = new Timer(delegate { Cleanup(); });
        }

        public static string PipeName
        {
            get
            {
                return "Sudo-Commands-For-" + WindowsIdentity.GetCurrent().User.ToString();
            }
        }

        static void Cleanup()
        {
            lock (ServerLock)
            {
                Listener.Close();
                Listener = null;

                ServersIdle.Set();
            }
        }

        static NamedPipeServerStream CreateServerStream()
        {
            var user = WindowsIdentity.GetCurrent().User;
            var security = new PipeSecurity();
            security.AddAccessRule(new PipeAccessRule(user, PipeAccessRights.FullControl, AccessControlType.Allow));
            security.SetOwner(user);
            security.SetGroup(user);

            IncrementServers();
            try
            {
                return new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    20,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    CommandLineLength,
                    CommandLineLength,
                    security);
            }
            catch (Exception)
            {
                DecrementServers();
                throw;
            }
        }

        static void DecrementServers()
        {
            lock (ServerLock)
            {
                ActiveServers--;
                if (ActiveServers == 0)
                {
                    NoActiveServers.Set();
                }
            }
        }

        static int Execute(ExecuteRequest request)
        {
            try
            {
                // Spawn off the child process.
                // Note that the command line here is magic. It *is* possible to screw it up. Please don't.
                //
                var startInfo = new ProcessStartInfo()
                {
                    FileName = String.Format("\"{0}\"", Assembly.GetEntryAssembly().Location),
                    Arguments = String.Format(
                        "-Exec {0} {1} {2}",
                        request.ClientPid,
                        Executor.MagicDelimiter,
                        request.CommandLine),
                    WorkingDirectory = request.WorkingDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                startInfo.EnvironmentVariables.Clear();
                foreach (var kvp in request.Environment)
                {
                    startInfo.EnvironmentVariables.Add(kvp.Key, kvp.Value);
                }


                Tracer.WriteLine(
                    "SI FileName: {0} \nArgs: {1}\nWorking: {2}",
                    startInfo.FileName,
                    startInfo.Arguments,
                    startInfo.WorkingDirectory);

                var process = Process.Start(startInfo);

                process.WaitForExit();

                Tracer.WriteLine("Done: {0}\n\n\n", process.ExitCode);
                return process.ExitCode;
            }
            finally
            {
                //
                // NOTE: There is a discontinuity here. Because we cannot rely on the finalizer we clear the "active 
                //       server" count once this method completes. In order to make the counts sane, this object can be 
                //       used only once.
                //
                lock (ServerLock)
                {
                    ActiveServers--;
                    if (ActiveServers == 0)
                    {
                        NoActiveServers.Set();
                    }
                }
            }
        }

        static void HandleIncomingConnection(NamedPipeServerStream server)
        {
            // This is not quite as good as it should be, but OH WELL
            //
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    using (server)
                    {
                        StreamWriter writer = new StreamWriter(server);
                        StreamReader reader = new StreamReader(server);
                        try
                        {
                            var request = ExecuteRequest.Read(reader);
                            if (request == null) { return; }

                            writer.WriteLine("{0}", Execute(request));
                            writer.WriteLine("OK");
                            writer.Flush();
                            server.Flush();
                        }
                        catch (Exception e)
                        {
                            writer.WriteLine("-1");
                            writer.WriteLine(e.Message);
                            writer.Flush();
                            server.Flush();
                        }
                    }
                }
                catch (IOException ioe)
                {
                    Tracer.WriteWarning("IO Error communicating with client: {0}", ioe);
                }
                finally
                {
                    DecrementServers();
                }
            });
        }

        static void IncomingConnectionCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously) { return; }
            IncomingConnectionLoop(result);
        }

        static void IncomingConnectionLoop(IAsyncResult result)
        {
            do
            {
                lock (ServerLock)
                {
                    if (Listener == null)
                    {
                        DecrementServers();
                        return;
                    }

                    Listener.EndWaitForConnection(result);
                    HandleIncomingConnection(Listener);

                    Listener = CreateServerStream();
                    result = Listener.BeginWaitForConnection(IncomingConnectionCallback, null);
                }
            }
            while (result.CompletedSynchronously);
        }

        static void IncrementServers()
        {
            lock (ServerLock)
            {
                ActiveServers++;
                NoActiveServers.Reset();
            }
        }

        static void ResetIdleTimer()
        {
            IdleTimer.Change(IdleTimeout, TimeSpan.FromMilliseconds(-1));
        }

        public static void Run()
        {
            Tracer.Start();
            NativeMethods.FreeConsole();

            // Step 1: Start the named pipe server.
            //
            Tracer.WriteLine("Starting to listen");
            StartListening();

            // Step 2: Wait until the idle timeout fires.
            //
            Tracer.WriteLine("Resetting the idle timer");
            ResetIdleTimer();
            Tracer.WriteLine("Waiting for idle");
            ServersIdle.WaitOne();

            // Step 4: Wait until all of running servers have finished.
            //
            Tracer.WriteLine("Waiting for all the active servers to finish");
            NoActiveServers.WaitOne();
            Tracer.WriteLine("Shutting down");
        }

        static void StartListening()
        {
            Listener = CreateServerStream();
            ThreadPool.QueueUserWorkItem(delegate
            {
                IAsyncResult result = Listener.BeginWaitForConnection(IncomingConnectionCallback, null);
                if (result.CompletedSynchronously)
                {
                    IncomingConnectionLoop(result);
                }
            });
        }







    }
}
