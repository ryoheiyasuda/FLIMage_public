using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagLensController
{
    public class TagLControl
    {
        const bool debugMessage = false;
        // TAG DRIVING KIT LIMITS
        const double DEF_CONN_TIMEOUT = 1.0;  //s

        const double MIN_FREQ = 30000.0;  // Hz
        const double MAX_FREQ = 525000.0;  // Hz
        const double DEFAULT_FREQ = 75000.0; // Hz

        const double MIN_AMP = 0.0;  // mV
        const double MAX_AMP = 30000.0; // mV (now is read from controller)
        const double MIN_AMP_100 = 0.0;   // pct
        const double MAX_AMP_100 = 100.0;  // pct
        const double DEFAULT_AMP = 25.0;  // pct

        // limits for the phase of VP1, VP2, VP3
        const double MIN_VPX_PHASE = 0.0;  // decidegree
        const double MAX_VPX_PHASE = 3600.0;  // decidegree
        const double MIN_VPX_WIDTH = 0.0;  // ns
        const double MAX_VPX_WIDTH = 2000.0;  // ns

        const double MAX_FRAME_RATE = 500000.0;  // Hz
        const double MIN_FRAME_RATE = 0.01;  // Hz
        const int MAX_F3_PLANES = 20;


        // packet constants
        const int _SOT_BYTE = 0x85;  // 133 Start of Transmission byte
        const int _SOT_SUB_1 = 0x80;  // 128 Substitute 1
        const int _SOT_SUB_2 = 0x05;  // 5 Substitute 2
        const int _SOT_SUB_3 = 0x00;  // 0 Substitute 3
        const int _CHECKSUM_MOD = 0x100;

        int baudrate = 9600;

        public SerialPort port;
        string comport;
        public Dictionary<string, byte[]> commands;

        public TagLControl()
        {
            commands = new Dictionary<string, byte[]>();
            commands.Add("CMD_SIN_FRQ_cHz", new byte[] { 0x00, 0x04, 4, 4 });
            commands.Add("CMD_GAT_NPULSES", new byte[] { 0x00, 0x05, 4, 8 });
            commands.Add("CMD_SIN_AMP_mV", new byte[] { 0x00, 0x06, 4, 4 });
            commands.Add("CMD_PULSE_OFF", new byte[] { 0x00, 0x07, 0, 4 });
            commands.Add("CMD_LOCK_AmpmMfN", new byte[] { 0x00, 0x08, 16, 4 });
            commands.Add("CMD_LOCK_LENS", new byte[] { 0x00, 0x08, 28, 4 });
            commands.Add("CMD_SIN_AMP_DEF", new byte[] { 0x00, 0x0b, 0, 4 });
            commands.Add("CMD_LED_SET_B", new byte[] { 0x00, 0x12, 4, 4 });
            commands.Add("CMD_FAN_SET_B", new byte[] { 0x00, 0x13, 4, 4 });
            commands.Add("CMD_PIEZO_SET_B", new byte[] { 0x00, 0x14, 4, 4 });
            commands.Add("CMD_VP1M_PH_DUR", new byte[] { 0x00, 0x15, 24, 24 });  // multiplane
            commands.Add("CMD_F3_NP_CYC_PHA_DUR", new byte[] { 0x00, 0x16, 56, 56 });  // F3
            commands.Add("CMD_RGB_PHA_36000_DUR_nS", new byte[] { 0x00, 0x19, 24, 24 });  // RGB
            commands.Add("CMD_CAL_ALL", new byte[] { 0x00, 0x17, 88, 88 });
            commands.Add("CMD_CAL_WRT", new byte[] { 0x00, 0x18, 0, 88 });
            commands.Add("CMD_MSG_UNKNOWN", new byte[] { 0x00, 0xf0, 0, 4 });
            commands.Add("INF_SIN_FRQ_cHz", new byte[] { 0x01, 0xa0, 0, 4 });
            commands.Add("INF_SIN_AMP_mV", new byte[] { 0x01, 0xa1, 0, 4 });
            commands.Add("INF_VIN_AMP_mV", new byte[] { 0x01, 0xa2, 0, 4 });
            commands.Add("INF_VMO_AMP_mV", new byte[] { 0x01, 0xa3, 0, 4 });
            commands.Add("INF_IMO_AMP_uA", new byte[] { 0x01, 0xa4, 0, 4 });
            commands.Add("INF_SIN_VRM_mV", new byte[] { 0x01, 0xa5, 0, 4 });
            commands.Add("INF_SIN_IRM_uA", new byte[] { 0x01, 0xa6, 0, 4 });
            commands.Add("INF_SIN_PHA_36000", new byte[] { 0x01, 0xa7, 0, 8 });
            commands.Add("INF_SIN_PQ_uWuVA", new byte[] { 0x01, 0xa8, 0, 8 });
            commands.Add("INF_LOCK_STATE", new byte[] { 0x01, 0xa9, 0, 4 });
            commands.Add("INF_VP1_PHA_36000_DUR_nS", new byte[] { 0x01, 0xaa, 0, 8 });
            commands.Add("INF_VP2_PHA_36000_DUR_nS", new byte[] { 0x01, 0xab, 0, 8 });
            commands.Add("INF_VP3_PHA_36000_DUR_nS", new byte[] { 0x01, 0xac, 0, 8 });
            commands.Add("INF_VP1_AMP_mV", new byte[] { 0x01, 0xad, 0, 4 });
            commands.Add("INF_VP2_AMP_mV", new byte[] { 0x01, 0xae, 0, 4 });
            commands.Add("INF_VP3_AMP_mV", new byte[] { 0x01, 0xaf, 0, 4 });
            commands.Add("INF_PULSE_STATE", new byte[] { 0x01, 0xb1, 0, 4 });
            commands.Add("INF_LNS_AFIVPPQ", new byte[] { 0x01, 0xb2, 0, 28 });
            commands.Add("INF_LED_SET_B", new byte[] { 0x01, 0xb4, 0, 4 });
            commands.Add("INF_FAN_SET_B", new byte[] { 0x01, 0xb5, 0, 4 });
            commands.Add("INF_DEBUG1_GET", new byte[] { 0x01, 0xb6, 0, 84 });
            commands.Add("INF_DEBUG2_GET", new byte[] { 0x01, 0xb7, 0, 64 });
            commands.Add("INF_GAT_NPULSES", new byte[] { 0x1, 0xb8, 0, 8 });
            commands.Add("INF_PIEZO_SET_B", new byte[] { 0x01, 0xba, 0, 4 });
            commands.Add("INF_VP1M_PH_DUR", new byte[] { 0x01, 0xbb, 0, 24 });
            commands.Add("INF_VER_8B", new byte[] { 0x01, 0xbc, 0, 8 });
            commands.Add("INF_F3_NP_CYC_PHA_DUR", new byte[] { 0x01, 0xbd, 0, 56 });
            commands.Add("INF_RGB_PHA_36000_DUR_nS", new byte[] { 0x01, 0xbf, 0, 24 });
            commands.Add("INF_CAL_ALL", new byte[] { 0x01, 0xbe, 0, 88 });
            commands.Add("INF_CLK_ALL", new byte[] { 0x01, 0xf1, 0, 40 });
            commands.Add("INF_MSG_LEN_UNK", new byte[] { 0x01, 0xfe, 0, 4 });
            commands.Add("INF_MSG_UNK_GET", new byte[] { 0x01, 0xff, 0, 4 });


        }

        public bool Connect(String COMPort)
        {
            comport = COMPort;
            try
            {
                port = new SerialPort(comport);

                port.BaudRate = 9600;
                port.DataBits = 8;
                port.StopBits = StopBits.One;
                port.NewLine = "\r";
                port.Handshake = Handshake.None;
                port.RtsEnable = true;
                port.DtrEnable = true;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000;

                port.Open();
                System.Threading.Thread.Sleep(10);
                return true;
            }
            catch (Exception SE)
            {
                Debug.WriteLine(SE.Message);
                return false;
            }
        }

        public void Disconnect()
        {
            port.Close();
        }

        public bool IsConnected()
        {
            if (port != null)
                return port.IsOpen;
            else
                return false;
        }

        public void ClearBuffer()
        {
            try
            {
                if (port != null && port.IsOpen)
                    port.ReadExisting();
            }
            catch (TimeoutException EX)
            {
                Debug.WriteLine("Clearing --- timeout" + EX.Message);
            }
        }

        private byte[] Read(int readBytes)
        {
            Stopwatch sw = new Stopwatch();
            Byte[] packet = new byte[readBytes];
            sw.Start();
            int bytesRead = 0;
            while (bytesRead == 0)
            {
                if (readBytes == 0)
                    break;
                try
                {
                    bytesRead = port.Read(packet, 0, readBytes);
                }
                catch (TimeoutException EX)
                {
                    if (debugMessage)
                        Debug.WriteLine("Read Time out..." + EX.Message);
                    return null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("No connection: " + ex.Message);
                    return null;
                }
            }

            if (debugMessage)
                Debug.WriteLine("Packet < {0} ({1} ms)", packet, sw.ElapsedMilliseconds);

            if (packet.Length == 0)
                ClearBuffer();

            return packet;
        }

        public void WriteBytes(byte[] packet)
        {
            ClearBuffer();
            try
            {
                if (port.IsOpen)
                    port.Write(packet, 0, packet.Length);
            }
            catch (TimeoutException TO)
            {
                if (debugMessage)
                    Debug.WriteLine("Write time out..." + TO.Message);
            }
        }

        public void WriteString(String str)
        {
            ClearBuffer();
            port.Write(str);
        }


        public void SendCommand(String command, int val)
        {
            SendCommand(command, new int[] { val }, out String str, out int[] values);
        }

        public void SendCommand(String command, int val, out String str, out int[] values)
        {
            SendCommand(command, new int[] { val }, out str, out values);
        }

        public void SendCommand(String command, int[] val, out String str, out int[] values)
        {
            Byte[] message;
            str = null;
            values = null;
            try
            {
                byte[] commandArray = commands[command];
                int tx_bytes = commandArray[2];
                int rx_bytes = commandArray[3];

                message = new byte[2 + tx_bytes];
                byte[] command_type_code = new byte[] { commandArray[0], commandArray[1] };
                Buffer.BlockCopy(command_type_code, 0, message, 0, 2);

                if (tx_bytes != 0)
                {
                    if (tx_bytes == 2)
                    {
                        //byte[] val_bytes = BitConverter.GetBytes((short)val[0]);
                        byte[] val_bytes = IntTo2Bytes(val[0]);
                        Buffer.BlockCopy(val_bytes, 0, message, 2, 2);
                    }
                    else if (tx_bytes == 4)
                    {
                        //byte[] val_bytes = BitConverter.GetBytes(val[0]);
                        byte[] val_bytes = IntTo4Bytes(val[0]);
                        Buffer.BlockCopy(val_bytes, 0, message, 2, 4);
                    }
                    else if (tx_bytes == 4 * val.Length)
                    {
                        for (int idx = 0; idx < val.Length; idx++)
                        {
                            byte[] val_bytes = IntTo4Bytes(val[idx]);
                            Buffer.BlockCopy(val_bytes, 0, message, 2 + idx * 4, 4);
                        }
                        //Buffer.BlockCopy(val, 0, message, 2, tx_bytes);
                    }
                    else
                    {
                        if (debugMessage)
                            Debug.WriteLine("Invalid Command Length");
                    }
                }

                SendMessage(message);
                System.Threading.Thread.Sleep(100);
                GetResponse(rx_bytes, out str, out values);

                if (str == "" && values == null)
                {
                    if (debugMessage)
                        Debug.WriteLine("TAG:send_command: To command: {0} Value: {1}", command, val[0]);
                }
                else if (str != "")
                {
                    if (debugMessage)
                        Debug.WriteLine("TAG:send_command: Response: {0}", str);
                }

                else if (values != null)
                {
                    if (debugMessage)
                    {
                        Debug.Write("TAG:send_command: To command:" + command + "Values =");
                        for (int i = 0; i < values.Length; i++)
                        {
                            Debug.Write(values[i].ToString() + ", ");
                        }
                        Debug.WriteLine("");
                    }
                }

            }
            catch (KeyNotFoundException)
            {
                if (debugMessage)
                    Debug.WriteLine("Key not found");
            }
        }

        public void GetResponse(int rx_bytes, out String str, out int[] values)
        {
            byte[] response = GetCobsMessage(rx_bytes);


            if (response == null)
            {
                str = "";
                values = null;
            }
            else if (response[0] == 1)
            {
                if (response[1] == commands["INF_VER_8B"][1])
                {
                    List<byte> resp = new List<byte>();
                    for (int index = 2; index < rx_bytes + 2; index++)
                    {
                        resp.Add(response[index]);
                    }
                    str = System.Text.Encoding.UTF8.GetString(resp.ToArray());
                    values = null;
                }
                else
                {
                    List<int> respInt = new List<int>();
                    for (int index = 2; index < rx_bytes + 2; index += 4)
                    {
                        int x = BitConverter.ToInt32(response, index);
                        int y = IntFrom4Bytes(response, index);
                        respInt.Add(y);
                    }
                    str = "";
                    values = respInt.ToArray();
                }
            }
            else
            {
                str = "";
                values = null;
                if (debugMessage)
                    Debug.WriteLine("TAG:_get_response:Wrong Message Type");
            }
        }

        private byte[] IntTo4Bytes(int x)
        {
            List<byte> n_list = new List<byte>();
            int[] nbitsArray = new int[] { 24, 16, 8, 0 };
            foreach (int nbits in nbitsArray)
                n_list.Add((byte)((x >> nbits) & 0xff));

            return n_list.ToArray();
        }

        private byte[] IntTo2Bytes(int x)
        {
            byte[] n_list = new byte[] { (byte)((x >> 8) & 0xff), (byte)((x >> 0) & 0xff) };
            return n_list;
        }

        private int IntFrom4Bytes(byte[] n_list, int startByte)
        {
            long x = (((((((long)n_list[startByte + 0] << 8) + (long)n_list[startByte + 1]) << 8) + (long)n_list[startByte + 2]) << 8) + (long)n_list[startByte + 3]);
            if (x > Math.Pow(2, 31))
                x = x - (long)Math.Pow(2, 32); //Should be fine with 1 << 32 but... it takes only integer.
            return (int)x;
        }

        public void SendMessage(byte[] message)
        {
            byte[] packet = MakePacket(message);
            WriteBytes(packet);
        }

        public byte[] GetMessage(int rx_bytes)
        {
            byte[] packet = Read(90);
            if (packet.Length > 0)
            {
                byte[] message = ExtractMessage(packet);
                if (message == null)
                    return null;
                else if (message.Length == rx_bytes + 2)
                {

                    if (debugMessage)
                        Debug.WriteLine("TAG: Message < % s" + message);
                    return message;
                }
                else
                {
                    if (debugMessage)
                        Debug.WriteLine("TAG: _get_message:Invalid message length: " + message);
                    return null;
                }
            }
            else
            {
                if (debugMessage)
                    Debug.WriteLine("TAG:_get_message:Zero length packet");
                return null;
            }
        }

        private byte[] MakePacket(byte[] message_in)
        {
            byte checksum = CalcCheckSum(message_in);
            List<byte> message = message_in.ToList();
            message.Add(checksum);
            message.Insert(0, (byte)message.Count);

            List<byte> packet = new List<byte>();
            //Begin packet creation, with header byte and length byte
            packet.Add(_SOT_BYTE);

            foreach (byte value in message)
            {
                //Convert 133 to (128+5) and 128 to (128+0) for signal.
                if (value == _SOT_BYTE)
                {
                    packet.Add(_SOT_SUB_1);
                    packet.Add(_SOT_SUB_2);
                }
                else if (value == _SOT_SUB_1)
                {
                    packet.Add(_SOT_SUB_1);
                    packet.Add(_SOT_SUB_3);
                }
                else
                    packet.Add(value);
            }
            return packet.ToArray();
        }

        private byte[] ExtractMessage(byte[] packet_in)
        {
            List<byte> packet = packet_in.ToList();

            if (packet[0] != _SOT_BYTE)
            {
                if (debugMessage)
                    Debug.WriteLine("TAG:_extract_message:Invalid first packet byte");
                return null;
            }
            packet = RemoveSubstitution(packet.ToArray()).ToList();
            if (packet == null)
                return null;

            if (packet[1] + 2 != packet.Count)
            {
                if (debugMessage)
                    Debug.WriteLine("TAG:_extract_message:Invalid packet length");
            }


            packet.RemoveAt(0);
            if (TestCheckSum(packet.ToArray(), packet[packet.Count - 1]))
            {
                packet.RemoveAt(0);
                return packet.ToArray();
            }
            else
                return null;

        }

        private byte[] RemoveSubstitution(byte[] packet_in)
        {
            List<byte> packet = packet_in.ToList();
            for (int i = 0; i < packet.Count; i++)
            {
                Byte value = packet[i];
                if (value == _SOT_SUB_1)
                {
                    if (packet[i + 1] == _SOT_SUB_3)
                        packet.RemoveAt(i + 1);
                    else if (packet[i + 1] == _SOT_SUB_2)
                    {
                        packet[i] = _SOT_BYTE;
                        packet.RemoveAt(i + 1);
                    }
                    else
                    {
                        if (debugMessage)
                            Debug.WriteLine("TAG:_remove_substitutions:Invalid SOT SUB");
                    }
                }
            }
            return packet.ToArray();
        }

        private void SendCobsMessage(byte[] message)
        {
            byte[] packet = CobsPacketFromMessage(message);
            if (packet != null)
                WriteBytes(packet);
        }

        private byte[] GetCobsMessage(int rx_bytes)
        {
            byte[] packet = Read(rx_bytes + 6);
            if (packet != null && packet.Length > 0)
            {
                byte[] message = MessageFromCobsPacket(packet);
                if (message == null)
                    return null;
                else if (message.Length == rx_bytes + 2)
                {
                    if (debugMessage)
                        Debug.WriteLine("TAG:_get_cobs_message:Message< " + message);
                    return message;
                }
                else
                {
                    if (debugMessage)
                        Debug.WriteLine("TAG:_get_cobs_message:Invalid message length: " + message);
                    return null;
                }
            }
            else
            {
                if (debugMessage)
                    Debug.WriteLine("TAG:_get_cobs_message:Zero length packet");
                return null;
            }
        }

        private byte[] CobsPacketFromMessage(byte[] message_in)
        {
            List<byte> message = message_in.ToList();
            if (message.Count > 253)
                return null;

            byte cks = GetCheckSum(message.ToArray());
            message.Add(cks);
            message.Insert(0, 0);
            List<byte> packet = message.ToArray().ToList();
            List<int> izeros = new List<int>();
            for (int i = 0; i < message.Count; i++)
            {
                if (message[i] == 0)
                    izeros.Add(i);
            }
            izeros.Add(message.Count);

            for (int i = 0; i < izeros.Count; i++)
            {
                packet[izeros[i]] = (byte)(izeros[i + 1] - izeros[i]);
            }

            packet.Insert(0, 0);
            packet.Add(0);

            return packet.ToArray();
        }



        private byte[] MessageFromCobsPacket(byte[] packet)
        {
            if (packet == null || packet.Length > 257 || packet[0] != 0 || packet[packet.Length - 1] != 0)
                return null;

            //Remove first and last.            
            List<byte> messageList = packet.ToList();
            messageList.RemoveAt(0);
            messageList.RemoveAt(messageList.Count - 1);

            int icode = 0;
            while (icode < messageList.Count)
            {
                byte inc = messageList[icode];
                if (inc != 0xff)
                {
                    messageList[icode] = 0;
                    icode += inc;
                }
                else
                {
                    messageList.RemoveAt(icode);
                    icode += inc - 1;
                }
            }
            // remove first SOF offset
            messageList.RemoveAt(0);
            Byte cks = messageList[messageList.Count - 1];
            //remove csk
            messageList.RemoveAt(messageList.Count - 1);

            byte[] message = messageList.ToArray();

            if (!TestCheckSum(message, cks))
                return null;

            return message;
        }

        private byte GetCheckSum(byte[] message)
        {
            int sum = 0;
            for (int i = 0; i < message.Length; i++)
                sum = sum + message[i];

            byte checksum = (byte)py_mod((_CHECKSUM_MOD - py_mod(sum, _CHECKSUM_MOD)), _CHECKSUM_MOD);
            return checksum;
        }

        private bool TestCheckSum(byte[] packet, byte cks)
        {
            int sum = 0;
            for (int i = 0; i < packet.Length; i++)
                sum = sum + packet[i];

            byte checksum = (byte)py_mod((_CHECKSUM_MOD - py_mod(sum, _CHECKSUM_MOD)), _CHECKSUM_MOD);
            if (checksum != cks)
                return false;  // invalid packet
            else
                return true;
        }


        private byte CalcCheckSum(byte[] message)
        {
            int sum = 0;
            for (int i = 0; i < message.Length; i++)
                sum = sum + (int)message[i];

            byte checksum = (byte)py_mod((_CHECKSUM_MOD - py_mod((1 + sum + message.Length), _CHECKSUM_MOD)), _CHECKSUM_MOD);

            //byte checksum = (byte)(_CHECKSUM_MOD - py_mod(py_mod(1 + sum + message.Length, _CHECKSUM_MOD), _CHECKSUM_MOD));
            return checksum;
        }


        int py_div(int a, int b)
        {
            if (a < 0)
                if (b < 0)
                    return -a / -b;
                else
                    return -(-a / b) - (-a % b != 0 ? 1 : 0);
            else if (b < 0)
                return -(a / -b) - (a % -b != 0 ? 1 : 0);
            else
                return a / b;
        }

        int py_mod(int a, int b)
        {
            if (a < 0)
                if (b < 0)
                    return -(-a % -b);
                else
                    return -a % b - (-a % -b != 0 ? 1 : 0);
            else if (b < 0)
                return -(a % -b) + (-a % -b != 0 ? 1 : 0);
            else
                return a % b;
        }
    }
}
