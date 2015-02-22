using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    class XBeeSerialConnection
    {
        private SerialPort port;
        private Queue<XBeeAPIFrame> Packets;
        private Queue<byte> receivedData;
        private List<byte> NextPacket;
        private bool Esc;
        public event ChangedEventHandler newPacket;
        public XBeeSerialConnection(String SerialPort, int BaudRate, bool Escaped)
        {
            port = new SerialPort(SerialPort, BaudRate, Parity.None, 8, StopBits.One);
            Esc = Escaped;
            receivedData = new Queue<byte>();
            NextPacket = new List<byte>();
            Packets = new Queue<XBeeAPIFrame>();
            port.DataReceived += serialPort_DataReceived;
            port.Open();
        }
        ~XBeeSerialConnection()
        {
            port.Dispose();
        }
        private void serialPort_DataReceived(object s, SerialDataReceivedEventArgs e)
        {
            getPortData();
        }
        private void getPortData()
        {
            byte[] data = new byte[port.BytesToRead];
            port.Read(data, 0, data.Length);
            data.ToList().ForEach(b => receivedData.Enqueue(b));
            processData();
        }
        private void processData()
        {
            // Determine if we have a "packet" in the queue
            while (receivedData.Count > 0)
            {
                byte Next = receivedData.Dequeue();
                if (Next == 0x7e)
                {
                    if (VerifyAndEscape(ref NextPacket))
                    {
                        XBeeAPIFrame Frame = new XBeeAPIFrame(NextPacket);
                        Packets.Enqueue(Frame);
                        OnPacketReceived(EventArgs.Empty);
                    }
                    NextPacket = new List<byte>();
                    NextPacket.Add(Next);
                }
                else
                {
                    NextPacket.Add(Next);
                    if (VerifyAndEscape(ref NextPacket))
                    {
                        XBeeAPIFrame Frame = new XBeeAPIFrame(NextPacket);
                        Packets.Enqueue(Frame);
                        OnPacketReceived(EventArgs.Empty);
                    }
                }
            }
        }

        protected virtual void OnPacketReceived(EventArgs e)
        {
            if (newPacket != null)
                newPacket(this, e);
        }
        private bool VerifyAndEscape(ref List<byte> Packet)
        {
            List<byte> EscapedPacket = new List<byte>();
            bool NextByteIsEscaped = false;
            int Length = 0;
            int Checksum = 0;

            if(Esc)
            {
                foreach (byte R in Packet)
                {
                    if (NextByteIsEscaped == false)
                    {
                        if (R == 0x7D)
                        {
                            NextByteIsEscaped = true;
                        }
                        else
                        {
                            EscapedPacket.Add(R);
                        }
                    }
                    else
                    {
                        EscapedPacket.Add((byte)((int)R ^ 0x20));
                        NextByteIsEscaped = false;
                    }
                }
            }
            else
            {
                foreach (byte R in Packet)
                {
                    EscapedPacket.Add(R);
                }
            }
            if (EscapedPacket.Count > 5)
            {
                if (EscapedPacket[0] == 0x7E)
                {
                    Length = EscapedPacket[1] << 8;
                    Length = Length + EscapedPacket[2];
                    if (EscapedPacket.Count - 4 == Length)
                    {
                        for (int i = 3; i < EscapedPacket.Count - 1; i++)
                        {
                            Checksum = Checksum + EscapedPacket[i];
                        }
                        Checksum = 0xFF - (Checksum & 0xFF);
                        if (EscapedPacket.Last() == (byte)Checksum)
                        {
                            Packet.Clear();
                            foreach (byte t in EscapedPacket)
                            {
                                Packet.Add(t);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public int getPacketCount()
        {
            return Packets.Count;
        }
        public XBeeAPIFrame getNextPacket()
        {
            if (Packets.Count > 0)
                return Packets.Dequeue();
            else
                return null;
        }

        public void SendPacket(XBeeAPIFrame Packet)
        {
            List<byte> T = Packet.getRawFrameData();
            if (Esc == true)
            {
                List<byte> Y = new List<byte>();
                Y.Add(0x7e);
                for (int i = 1; i < T.Count; i++)
                {
                    byte b = T[i];
                    if(b != 0x7e && b != 0x7d && b != 0x11 && b != 0x13)
                    {
                        Y.Add(b);
                    }
                    else
                    {
                        Y.Add(0x7d);
                        Y.Add((byte)((int)b ^ 0x20));
                    }
                }
                T = Y;
            }
            port.Write(T.ToArray(), 0, T.Count);
            //char[] R = System.Text.Encoding.ASCII.GetString(T.ToArray()).ToCharArray();
            //string R = GetString(T.ToArray());
            //port.Write(R, 0, T.Count);
        }

        string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
