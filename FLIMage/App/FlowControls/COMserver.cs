using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace FLIMage.FlowControls
{
    // Multi-client pipe server (stage 2).
    //
    // Each client owns two named pipes: "FLIMageR" (FLIMage -> client, events)
    // and "FLIMageW" (client -> FLIMage, commands and their replies). One
    // accept loop per pipe name keeps re-arming WaitForConnection so any number
    // of clients (up to MAX_INSTANCES) can connect while the server is on.
    //
    // Pairing the two pipes of one client uses arrival order (v1): the first
    // unmatched "FLIMageR" connection is paired with the first unmatched
    // "FLIMageW" connection. The handshake carries no client ID, so two clients
    // connecting at the exact same moment could in principle cross their pipes;
    // existing Python/Matlab clients connect both pipes back-to-back, which
    // pairs correctly. If simultaneous connects become a problem, add a client
    // ID to the handshake (v2) and fall back to arrival order when absent.
    //
    // Commands are executed through FLIMage_Event's serializing worker: each
    // session's receive thread calls CommandHandler (blocking until the worker
    // produces the reply) and writes the reply back on its own command pipe, so
    // replies always reach the client that sent the command. Events are
    // broadcast to every connected session.
    // by Kengo(Claude) 06-11-2026
    public class COMserver
    {
        const int MAX_INSTANCES = 254;

        FLIMageMain flimage;

        // True while accept loops should keep listening for new clients.
        public volatile bool Listening = false;

        // Kept as properties so existing call sites (event sending, status
        // display) still read "is at least one client connected".
        public Boolean connected { get { return !sessions.IsEmpty; } }
        public Boolean connectedR { get { return !sessions.IsEmpty; } }

        // Executes one received command and returns the reply. Assigned by
        // FLIMage_Event; called concurrently from each session's receive
        // thread (the handler itself serializes execution).
        public Func<String, String> CommandHandler;

        String SNameR = "FLIMageW"; //Name must be opposite from Client for writing and reading
        String SName = "FLIMageR";

        readonly ConcurrentDictionary<int, ClientSession> sessions = new ConcurrentDictionary<int, ClientSession>();
        int sessionCounter = 0;

        // Pipe halves that completed the handshake and are waiting for their
        // partner pipe to form a session. Guarded by pairLock.
        readonly object pairLock = new object();
        readonly Queue<PendingHalf> waitingW = new Queue<PendingHalf>();
        readonly Queue<PendingHalf> waitingR = new Queue<PendingHalf>();

        // Pipe instances currently blocked in WaitForConnection, so Close()
        // can dispose them to unblock the accept loops. Guarded by acceptLock.
        readonly object acceptLock = new object();
        NamedPipeServerStream pendingServerW, pendingServerR;

        class PendingHalf
        {
            public NamedPipeServerStream Pipe;
            public StreamString Ss;
        }

        public class ClientSession
        {
            public readonly int Id;
            public readonly NamedPipeServerStream PipeW; // "FLIMageR": events to client
            public readonly NamedPipeServerStream PipeR; // "FLIMageW": commands from client
            public readonly StreamString SsW;
            public readonly StreamString SsR;

            // Serializes event writes to SsW (broadcasts can come from
            // concurrent threads). Replies on SsR need no lock: only this
            // session's receive thread writes there.
            public readonly object WriteLock = new object();

            public volatile bool Connected;
            public Thread ReceiveThread;

            public ClientSession(int id, NamedPipeServerStream pipeW, StreamString ssW,
                                         NamedPipeServerStream pipeR, StreamString ssR)
            {
                Id = id;
                PipeW = pipeW;
                SsW = ssW;
                PipeR = pipeR;
                SsR = ssR;
                Connected = true;
            }

            // Disposing the pipes unblocks a pending synchronous ReadString()
            // on the receive thread (same shutdown strategy as the
            // single-client server). Safe to call more than once.
            public void Dispose()
            {
                Connected = false;
                try { PipeW.Close(); PipeW.Dispose(); }
                catch (Exception ex) { Debug.WriteLine("Closing session pipeW: " + ex.Message); }
                try { PipeR.Close(); PipeR.Dispose(); }
                catch (Exception ex) { Debug.WriteLine("Closing session pipeR: " + ex.Message); }
            }
        }

        public COMserver(FLIMageMain f)
        {
            flimage = f;
        }

        public int NConnectedClients { get { return sessions.Count; } }

        public void start()
        {
            if (Listening)
                return;
            Listening = true;

            flimage.script?.status_ComServer(false, FLIMage_Event.CommandReceivedFrom.Client);
            flimage.script?.status_ComServer(false, FLIMage_Event.CommandReceivedFrom.FLIMage);

            Thread tW = new Thread(() => AcceptLoop(SName, true));
            tW.IsBackground = true;
            tW.Name = "PipeAccept-" + SName;
            tW.Start();

            Thread tR = new Thread(() => AcceptLoop(SNameR, false));
            tR.IsBackground = true;
            tR.Name = "PipeAccept-" + SNameR;
            tR.Start();
        }

        // One loop per pipe name. Re-arms after every accepted connection so
        // additional clients can connect at any time while the server is on.
        void AcceptLoop(String pipeName, bool isWriteSide)
        {
            while (Listening)
            {
                NamedPipeServerStream pipe;
                try
                {
                    pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, MAX_INSTANCES,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                }
                catch (IOException ex)
                {
                    Debug.WriteLine("Pipe busy: " + ex.Message);
                    break;
                }

                lock (acceptLock)
                {
                    if (!Listening)
                    {
                        pipe.Dispose();
                        break;
                    }
                    if (isWriteSide) pendingServerW = pipe; else pendingServerR = pipe;
                }

                try
                {
                    pipe.WaitForConnection();
                }
                catch (ObjectDisposedException e)
                {
                    Debug.WriteLine("Already Closed: " + e.Message);
                    break;
                }
                catch (IOException e)
                {
                    Debug.WriteLine("Pipe busy: " + e.Message);
                    try { pipe.Dispose(); } catch { }
                    continue;
                }
                finally
                {
                    lock (acceptLock)
                    {
                        if (isWriteSide) pendingServerW = null; else pendingServerR = null;
                    }
                }

                if (!Listening)
                {
                    try { pipe.Dispose(); } catch { }
                    break;
                }

                StreamString ss1 = new StreamString(pipe);
                if (!handShake(ss1))
                {
                    Debug.WriteLine("Handshake failed: " + pipeName);
                    try { pipe.Dispose(); } catch { }
                    continue;
                }

                Debug.WriteLine("Connected: " + pipeName);
                PairHalf(isWriteSide, new PendingHalf { Pipe = pipe, Ss = ss1 });
            }
        }

        // Arrival-order pairing (v1): join this half with the oldest waiting
        // half of the other pipe, or queue it until its partner arrives.
        void PairHalf(bool isWriteSide, PendingHalf half)
        {
            ClientSession session = null;
            lock (pairLock)
            {
                // A handshake can finish while Close() is tearing down. Without
                // this check the half would be queued after Close() cleared the
                // queues and could be mispaired with a client of a later start().
                if (!Listening)
                {
                    try { half.Pipe.Dispose(); } catch { }
                    return;
                }

                Queue<PendingHalf> own = isWriteSide ? waitingW : waitingR;
                Queue<PendingHalf> other = isWriteSide ? waitingR : waitingW;

                // Drop waiting halves whose client already went away, so a
                // stale half cannot be paired with the next client.
                while (other.Count > 0 && !other.Peek().Pipe.IsConnected)
                {
                    PendingHalf dead = other.Dequeue();
                    try { dead.Pipe.Dispose(); } catch { }
                }

                if (other.Count > 0)
                {
                    PendingHalf partner = other.Dequeue();
                    PendingHalf w = isWriteSide ? half : partner;
                    PendingHalf r = isWriteSide ? partner : half;
                    int id = Interlocked.Increment(ref sessionCounter);
                    session = new ClientSession(id, w.Pipe, w.Ss, r.Pipe, r.Ss);
                }
                else
                {
                    own.Enqueue(half);
                }
            }

            if (session != null)
                AddSession(session);
        }

        void AddSession(ClientSession session)
        {
            sessions[session.Id] = session;

            session.ReceiveThread = new Thread(() => ReceiveLoop(session));
            session.ReceiveThread.IsBackground = true;
            session.ReceiveThread.Name = "PipeReceive-" + session.Id;
            session.ReceiveThread.Start();

            Debug.WriteLine("PIPE client #" + session.Id + " connected. Total: " + sessions.Count);
            UpdateConnectionStatus();
        }

        void RemoveSession(ClientSession session)
        {
            if (sessions.TryRemove(session.Id, out _))
                Debug.WriteLine("PIPE client #" + session.Id + " disconnected. Total: " + sessions.Count);
            session.Dispose();
            UpdateConnectionStatus();
        }

        void UpdateConnectionStatus()
        {
            int n = sessions.Count;
            flimage.script?.status_ComServer(n > 0, FLIMage_Event.CommandReceivedFrom.Client, n);
            flimage.script?.status_ComServer(n > 0, FLIMage_Event.CommandReceivedFrom.FLIMage, n);
        }

        Boolean handShake(StreamString ss0)
        {
            Boolean result = false;
            try
            {
                ss0.WriteString("FLIMage");
                String received = ss0.ReadString();
                if (received == "FLIMage")
                {
                    result = true;
                    ss0.WriteString("Connected");
                }
            }
            catch (IOException e)
            {
                Debug.WriteLine("ERROR: {0}", e.Message);
            }
            return result;
        }

        // Request/reply loop for one client. Blocking on CommandHandler is
        // fine: this is the session's own thread, and the handler routes the
        // command through the serializing worker, so the reply written here
        // always belongs to the command this client sent.
        void ReceiveLoop(ClientSession session)
        {
            try
            {
                while (Listening && session.Connected)
                {
                    String received;
                    try
                    {
                        received = session.SsR.ReadString();
                    }
                    catch (IOException e)
                    {
                        Debug.WriteLine("Failed receiving commands: " + e.Message);
                        break;
                    }

                    if (!Listening || !session.Connected)
                        break;

                    // ReadString returns "" when the pipe is closed or broken
                    // (EOF). Treat it as a disconnect of this client only.
                    if (String.IsNullOrEmpty(received))
                        break;

                    // "Disconnect" from a pipe client ends only this session.
                    // The command table's "Disconnect" case (TurnOnServer(false))
                    // would shut down the whole server and kick every other
                    // client, so it must not reach the worker from here; it
                    // remains available from the RemoteControl window.
                    if (received == "Disconnect")
                        break;

                    Func<String, String> handler = CommandHandler;
                    String replyMessage = handler != null ? handler(received) : "";

                    if (session.SsR.WriteString(replyMessage) == 0)
                        break;
                }
            }
            finally
            {
                RemoveSession(session);
            }
        }

        // Send an event to every connected client. Named sendCommand
        // historically; Broadcast is the multi-client behavior.
        public void sendCommand(string str)
        {
            Broadcast(str);
        }

        public void Broadcast(string str)
        {
            if (String.IsNullOrEmpty(str) || sessions.IsEmpty)
                return;

            Thread th = new Thread(() => BroadcastThread(str));
            th.IsBackground = true;
            th.Start();
        }

        private void BroadcastThread(string str)
        {
            foreach (ClientSession session in sessions.Values)
            {
                int ret;
                lock (session.WriteLock)
                {
                    ret = session.SsW.WriteString(str);
                }
                if (ret == 0)
                {
                    Debug.WriteLine("Failed PIPE broadcast to client #" + session.Id + ": " + str);
                    RemoveSession(session);
                }
            }
        }

        public void Restart()
        {
            Close();
            start();
        }

        public void Close()
        {
            Debug.WriteLine("PIPE close signal received.");

            // Stop accept loops and receive loops before disposing streams.
            Listening = false;

            // Disposing the pending pipe servers unblocks WaitForConnection()
            // (throws ObjectDisposedException, which AcceptLoop catches), and
            // disposing each session's pipes unblocks its synchronous
            // ReadString(). Same strategy as the single-client server: never
            // wait on the blocked threads themselves, just dispose and let
            // them exit. by Kengo(Claude) 06-09-2026 / 06-11-2026
            lock (acceptLock)
            {
                if (pendingServerW != null)
                {
                    try { pendingServerW.Dispose(); }
                    catch (Exception ex) { Debug.WriteLine("Closing pipeServer: " + ex.Message); }
                    pendingServerW = null;
                }
                if (pendingServerR != null)
                {
                    try { pendingServerR.Dispose(); }
                    catch (Exception ex) { Debug.WriteLine("Closing pipeServerR: " + ex.Message); }
                    pendingServerR = null;
                }
            }

            lock (pairLock)
            {
                while (waitingW.Count > 0)
                {
                    try { waitingW.Dequeue().Pipe.Dispose(); } catch { }
                }
                while (waitingR.Count > 0)
                {
                    try { waitingR.Dequeue().Pipe.Dispose(); } catch { }
                }
            }

            foreach (ClientSession session in sessions.Values)
                RemoveSession(session);
        }

        public class StreamString
        {
            private Stream ioStream;
            private Encoding streamEncoding;

            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = Encoding.UTF8;

                //ioStream.WriteTimeout = 100;
                //ioStream.ReadTimeout = 100;
            }

            public string ReadString()
            {
                String result = "";
                try
                {
                    int len, len1;
                    len = ioStream.ReadByte() * 256;
                    len += ioStream.ReadByte();
                    len1 = len;
                    if (len < 1)
                        len1 = 1;

                    byte[] inBuffer = new byte[len1];

                    if (len > 0)
                    {
                        ioStream.Read(inBuffer, 0, len);
                        result = streamEncoding.GetString(inBuffer);
                    }
                }
                catch (TimeoutException e)
                {
                    Debug.WriteLine("Time out" + e);
                }
                catch (ObjectDisposedException e)
                {
                    Debug.WriteLine("Read String error: " + e);
                }

                return result;
            }

            public int WriteString(object obj)
            {
                byte[] outBuffer = null;

                if (obj == null)
                    return 0;

                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    outBuffer = ms.ToArray();
                }

                return WriteString(outBuffer);
            }

            public int WriteString(String outString)
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                return WriteString(outBuffer);
            }

            public int WriteString(byte[] outBuffer)
            {
                int len = outBuffer.Length;
                if (len <= 0)
                    return 0;

                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }

                try
                {
                    ioStream.WriteByte((byte)(len / 256));
                    ioStream.WriteByte((byte)(len & 255));
                    ioStream.Write(outBuffer, 0, len);

                    ioStream.Flush();

                    return len + 2;
                }
                catch (OperationCanceledException e)
                {
                    Debug.WriteLine("Canceled" + e);
                    return 0;
                }
                catch (TimeoutException e)
                {
                    Debug.WriteLine("Time out" + e);
                    return 0;
                }
                catch (IOException e)
                {
                    Debug.WriteLine("Write String error IOException: " + e);
                    return 0;
                }
                catch (ObjectDisposedException e)
                {
                    Debug.WriteLine("Write String error: " + e);
                    return 0;
                }
            }//WriteString
        } //Class streaming

    }

} //Name space
