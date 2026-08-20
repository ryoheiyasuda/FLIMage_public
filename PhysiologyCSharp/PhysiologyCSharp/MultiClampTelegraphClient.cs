using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PhysiologyCSharp
{
    internal sealed class MultiClampTelegraphClient : IDisposable
    {
        private const int WM_COPYDATA = 0x004A;
        private const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
        private const int MIN_TELEGRAPH_PACKET_SIZE = 128;
        private const uint MCTG_API_VERSION_700A = 5;
        private const uint MCTG_API_VERSION_700B = 13;
        private const uint MCTG_HW_TYPE_MC700A = 0;
        private const uint MCTG_HW_TYPE_MC700B = 1;
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        private static readonly int TelegraphPacketSize = Marshal.SizeOf(typeof(MCTelegraphData));

        private readonly object stateLock = new object();
        private readonly object refreshLock = new object();
        private readonly Dictionary<int, TelegraphState> amplifiers = new Dictionary<int, TelegraphState>();
        private readonly List<int> amplifierOrder = new List<int>();
        private readonly ManualResetEventSlim windowReady = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim shutdownComplete = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim packetReceived = new ManualResetEventSlim(false);
        private readonly Thread messageThread;

        private MessageWindow messageWindow;
        private Exception startupException;
        private bool disposed;

        private int mctgOpenMessage;
        private int mctgCloseMessage;
        private int mctgRequestMessage;
        private int mctgReconnectMessage;
        private int mctgBroadcastMessage;
        private int mctgIdMessage;
        private int shutdownMessage;
        private int readyMessage;

        public MultiClampTelegraphClient()
        {
            if (TelegraphPacketSize != 256)
            {
                throw new InvalidOperationException("Unexpected MC_TELEGRAPH_DATA layout size.");
            }

            messageThread = new Thread(MessageLoop)
            {
                IsBackground = true,
                Name = "MultiClamp Telegraph Client"
            };
            messageThread.SetApartmentState(ApartmentState.STA);
            messageThread.Start();

            if (!windowReady.Wait(TimeSpan.FromSeconds(5)))
            {
                throw new TimeoutException("Timed out while starting the MultiClamp telegraph client.");
            }

            if (startupException != null)
            {
                throw new InvalidOperationException("Failed to start the MultiClamp telegraph client.", startupException);
            }
        }

        public TelegraphState[] GetAmplifierStates()
        {
            ThrowIfDisposed();

            RefreshTelegraph();

            lock (stateLock)
            {
                var results = new TelegraphState[amplifierOrder.Count];
                for (int i = 0; i < amplifierOrder.Count; i++)
                {
                    results[i] = amplifiers[amplifierOrder[i]].Clone();
                }

                return results;
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            try
            {
                var handle = GetWindowHandle();
                if (handle != IntPtr.Zero && shutdownMessage != 0)
                {
                    NativeMethods.PostMessage(handle, shutdownMessage, IntPtr.Zero, IntPtr.Zero);
                    shutdownComplete.Wait(TimeSpan.FromSeconds(5));
                }
            }
            finally
            {
                windowReady.Dispose();
                shutdownComplete.Dispose();
                packetReceived.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private void MessageLoop()
        {
            try
            {
                RegisterMessages();
                messageWindow = new MessageWindow(this);
                NativeMethods.PostMessage(messageWindow.Handle, readyMessage, IntPtr.Zero, IntPtr.Zero);
                Application.Run();
            }
            catch (Exception ex)
            {
                startupException = ex;
                windowReady.Set();
            }
            finally
            {
                if (messageWindow != null)
                {
                    messageWindow.DestroyWindowHandle();
                    messageWindow = null;
                }

                shutdownComplete.Set();
            }
        }

        private void RegisterMessages()
        {
            mctgOpenMessage = RegisterWindowMessage("MultiClampTelegraphOpenMsg");
            mctgCloseMessage = RegisterWindowMessage("MultiClampTelegraphCloseMsg");
            mctgRequestMessage = RegisterWindowMessage("MultiClampTelegraphRequestMsg");
            mctgReconnectMessage = RegisterWindowMessage("MultiClampTelegraphReconnectMsg");
            mctgBroadcastMessage = RegisterWindowMessage("MultiClampTelegraphBroadcastMsg");
            mctgIdMessage = RegisterWindowMessage("MultiClampTelegraphIdMsg");
            shutdownMessage = RegisterWindowMessage("MCT_SHUTDOWN");
            readyMessage = RegisterWindowMessage("FLIMage.MultiClampTelegraphClient.Ready");
        }

        private static int RegisterWindowMessage(string name)
        {
            int messageId = NativeMethods.RegisterWindowMessage(name);
            if (messageId == 0)
            {
                throw new InvalidOperationException("Failed to register window message: " + name);
            }

            return messageId;
        }

        private void RefreshTelegraph()
        {
            lock (refreshLock)
            {
                packetReceived.Reset();

                int initialCount = GetAmplifierCount();
                int waitMilliseconds = initialCount == 0 ? 500 : 150;

                for (int i = 0; i < 3; i++)
                {
                    Broadcast();
                    Thread.Sleep(20);
                }

                var timeoutAt = Environment.TickCount + waitMilliseconds;
                while (Environment.TickCount < timeoutAt)
                {
                    int remaining = timeoutAt - Environment.TickCount;
                    if (remaining <= 0)
                    {
                        break;
                    }

                    packetReceived.Wait(Math.Min(remaining, 50));
                    if (GetAmplifierCount() > 0 || initialCount > 0)
                    {
                        Thread.Sleep(20);
                        break;
                    }
                }
            }
        }

        private void Broadcast()
        {
            var handle = GetWindowHandle();
            if (handle == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.PostMessage(HWND_BROADCAST, mctgBroadcastMessage, handle, IntPtr.Zero);
        }

        private void OpenConnection(IntPtr id)
        {
            var handle = GetWindowHandle();
            if (handle == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.PostMessage(HWND_BROADCAST, mctgOpenMessage, handle, id);
        }

        private void CloseConnection(IntPtr id)
        {
            var handle = GetWindowHandle();
            if (handle == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.PostMessage(HWND_BROADCAST, mctgCloseMessage, handle, id);
        }

        private void RequestTelegraph(IntPtr id)
        {
            var handle = GetWindowHandle();
            if (handle == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.PostMessage(HWND_BROADCAST, mctgRequestMessage, handle, id);
        }

        private IntPtr GetWindowHandle()
        {
            return messageWindow != null ? messageWindow.Handle : IntPtr.Zero;
        }

        private void CloseAllConnections()
        {
            List<int> ids;
            lock (stateLock)
            {
                ids = new List<int>(amplifierOrder);
            }

            foreach (int id in ids)
            {
                CloseConnection(new IntPtr(id));
            }
        }

        private bool TryHandleMessage(ref Message message)
        {
            if (message.Msg == readyMessage)
            {
                Broadcast();
                windowReady.Set();
                message.Result = IntPtr.Zero;
                return true;
            }

            if (message.Msg == shutdownMessage)
            {
                CloseAllConnections();
                Application.ExitThread();
                message.Result = IntPtr.Zero;
                return true;
            }

            if (message.Msg == mctgIdMessage)
            {
                RequestTelegraph(message.LParam);
                OpenConnection(message.LParam);
                message.Result = new IntPtr(1);
                return true;
            }

            if (message.Msg == mctgReconnectMessage)
            {
                RequestTelegraph(message.LParam);
                message.Result = new IntPtr(1);
                return true;
            }

            if (message.Msg == WM_COPYDATA)
            {
                HandleCopyData(message.LParam);
                message.Result = new IntPtr(1);
                return true;
            }

            return false;
        }

        private void HandleCopyData(IntPtr copyDataPtr)
        {
            if (copyDataPtr == IntPtr.Zero)
            {
                return;
            }

            var copyData = (CopyDataStruct)Marshal.PtrToStructure(copyDataPtr, typeof(CopyDataStruct));
            if (copyData.dwData != new IntPtr(mctgRequestMessage) || copyData.lpData == IntPtr.Zero || copyData.cbData < MIN_TELEGRAPH_PACKET_SIZE)
            {
                return;
            }

            var packet = ReadTelegraphData(copyData.lpData, copyData.cbData);

            // Newer Commander builds can increment uVersion while keeping the
            // packet prefix layout compatible. The native DLL accepts that and
            // reads the common fields, so the managed path should do the same.
            StorePacket(packet);
        }

        private static MCTelegraphData ReadTelegraphData(IntPtr data, int length)
        {
            int bytesToCopy = Math.Min(length, TelegraphPacketSize);
            byte[] buffer = new byte[TelegraphPacketSize];
            Marshal.Copy(data, buffer, 0, bytesToCopy);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return (MCTelegraphData)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MCTelegraphData));
            }
            finally
            {
                handle.Free();
            }
        }

        private void StorePacket(MCTelegraphData packet)
        {
            int id = ComputeAmplifierId(packet);
            var nextState = new TelegraphState
            {
                ID = id,
                mode = (int)packet.uOperatingMode,
                primary_gain = packet.dAlpha,
                scaleFactor = packet.dScaleFactor,
                LPF_cutoff = packet.dLPFCutoff,
                external_cmd_sensitivity = packet.dExtCmdSens,
                second_alpha = packet.dSecondaryAlpha,
                second_LPF_cutoff = packet.dSecondaryLPFCutoff,
                AppVersion = CleanString(packet.szAppVersion),
                FirmwareVersion = CleanString(packet.szFirmwareVersion),
                DspVersion = CleanString(packet.szDSPVersion),
                SerialNumber = CleanString(packet.szSerialNumber)
            };

            lock (stateLock)
            {
                if (!amplifiers.ContainsKey(id))
                {
                    amplifierOrder.Add(id);
                }

                amplifiers[id] = nextState;
            }

            packetReceived.Set();
        }

        private static int ComputeAmplifierId(MCTelegraphData packet)
        {
            if (packet.uHardwareType == MCTG_HW_TYPE_MC700A)
            {
                return Pack700ASignalIds(packet.uComPortID, packet.uAxoBusID, packet.uChannelID);
            }

            if (packet.uHardwareType == MCTG_HW_TYPE_MC700B)
            {
                uint serialNumber = 0;
                uint.TryParse(CleanString(packet.szSerialNumber), NumberStyles.Integer, CultureInfo.InvariantCulture, out serialNumber);
                return Pack700BSignalIds(serialNumber, packet.uChannelID);
            }

            return 0;
        }

        private static int Pack700ASignalIds(uint comPortId, uint axoBusId, uint channelId)
        {
            uint packed = (comPortId & 0x000000FF)
                | ((axoBusId & 0x000000FF) << 8)
                | ((channelId & 0x0000FFFF) << 16);
            return unchecked((int)packed);
        }

        private static int Pack700BSignalIds(uint serialNumber, uint channelId)
        {
            uint packed = (serialNumber & 0x0FFFFFFF)
                | ((channelId & 0x0000000F) << 28);
            return unchecked((int)packed);
        }

        private static string CleanString(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.TrimEnd('\0', ' ');
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(MultiClampTelegraphClient));
            }
        }

        private int GetAmplifierCount()
        {
            lock (stateLock)
            {
                return amplifierOrder.Count;
            }
        }

        private sealed class MessageWindow : NativeWindow
        {
            private readonly MultiClampTelegraphClient owner;

            public MessageWindow(MultiClampTelegraphClient owner)
            {
                this.owner = owner;
                CreateHandle(new CreateParams
                {
                    Caption = "MCT_Message_Only",
                    Style = WS_OVERLAPPEDWINDOW
                });
            }

            protected override void WndProc(ref Message m)
            {
                if (owner.TryHandleMessage(ref m))
                {
                    return;
                }

                base.WndProc(ref m);
            }

            public void DestroyWindowHandle()
            {
                if (Handle != IntPtr.Zero)
                {
                    DestroyHandle();
                }
            }
        }

        internal sealed class TelegraphState
        {
            public int ID;
            public int mode;
            public double primary_gain;
            public double scaleFactor;
            public double LPF_cutoff;
            public double external_cmd_sensitivity;
            public double second_alpha;
            public double second_LPF_cutoff;
            public string AppVersion;
            public string FirmwareVersion;
            public string DspVersion;
            public string SerialNumber;

            public TelegraphState Clone()
            {
                return (TelegraphState)MemberwiseClone();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CopyDataStruct
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        private struct MCTelegraphData
        {
            public uint uVersion;
            public uint uStructSize;
            public uint uComPortID;
            public uint uAxoBusID;
            public uint uChannelID;
            public uint uOperatingMode;
            public uint uScaledOutSignal;
            public double dAlpha;
            public double dScaleFactor;
            public uint uScaleFactorUnits;
            public double dLPFCutoff;
            public double dMembraneCap;
            public double dExtCmdSens;
            public uint uRawOutSignal;
            public double dRawScaleFactor;
            public uint uRawScaleFactorUnits;
            public uint uHardwareType;
            public double dSecondaryAlpha;
            public double dSecondaryLPFCutoff;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string szAppVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string szFirmwareVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string szDSPVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string szSerialNumber;
            public double dSeriesResistance;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 76)]
            public byte[] pcPadding;
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern int RegisterWindowMessage(string lpString);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        }
    }
}
