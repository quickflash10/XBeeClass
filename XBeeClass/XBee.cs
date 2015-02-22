using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XBeeClass
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);
    public class XBee
    {
        private XBeeSerialConnection XBSerial;
        XBeeModule Me = new XBeeModule();
        List<XBeeModule> Modules = new List<XBeeModule>();
        Queue<XBeeAPIFrame> GenericFrames = new Queue<XBeeAPIFrame>();
        Queue<TransmitStatus> TransmitFrames = new Queue<TransmitStatus>();
        Queue<ATResponse> ATResponseFrames = new Queue<ATResponse>();
        Queue<ModemStatus> ModemStatusFrames = new Queue<ModemStatus>();
        Queue<ATRemoteResponse> UnknownATRemoteResponseFrames = new Queue<ATRemoteResponse>();
        Queue<ReceivePacket> UnknownReceivePacketFrames = new Queue<ReceivePacket>();

        bool initialOutSideView = false;
        public event ChangedEventHandler newPacket;
        public XBee(String SerialPort, int BaudRate, bool Escaped)
        {
            XBSerial = new XBeeSerialConnection(SerialPort,BaudRate, Escaped);
            XBSerial.newPacket += XBSerial_newPacket;
            ATCommand SH = new ATCommand("SH", 1);
            sendPacket(SH);
            Thread.Sleep(500);
            ATCommand SL = new ATCommand("SL", 2);
            sendPacket(SL);
            Thread.Sleep(500);
            ATCommand NI = new ATCommand("NI", 3);
            sendPacket(NI);
            Thread.Sleep(500);
            ATCommand ND = new ATCommand("ND", 4);
            sendPacket(ND);
            Thread.Sleep(500);
        }
        protected virtual void OnPacketReceived(EventArgs e)
        {
            if (newPacket != null)
                newPacket(this, e);
        }
        private void XBSerial_newPacket(object sender, EventArgs e)
        {
            while (XBSerial.getPacketCount() > 0)
            {
                XBeeAPIFrame R = XBSerial.getNextPacket();
                byte API = R.getAPIID();
                switch (API)
                {
                    case 0x88: //ATResponse
                        ATResponse T = new ATResponse(R);
                        if(T.getATCommand() == "SH")
                        {
                            Me.SetAddress64(SH: T.getDataBytes());
                        }
                        if (T.getATCommand() == "SL")
                        {
                            Me.SetAddress64(SL: T.getDataBytes());
                        }
                        if (T.getATCommand() == "NI")
                        {
                            Me.Update(NI: System.Text.Encoding.ASCII.GetString(T.getDataBytes()));
                        }
                        if (T.getATCommand() == "ND")
                        {
                            byte[] Info = T.getDataBytes();
                            bool found = false;
                            XBeeModule Modulus;
                            //Since this is an ND response we will create a module if needed and update the existing item if not needed
                            /*
                            MY (2 Bytes)
                            SH (4 Bytes)
                            SL (4 Bytes)
                            NI (Variable length)
                            PARENT_NETWORK ADDRESS (2 Bytes)
                            DEVICE_TYPE (1 Byte: 0 = Coord, 1 = Router, 2 = End Device)
                            STATUS(1 Byte: Reserved)
                            PROFILE_ID(2 Bytes)
                            MANUFACTURER_ID(2 Bytes)
                            */
                            int NILength = Info.Length - 18;
                            byte[] MY = new byte[] { Info[0], Info[1] };
                            byte[] SH = new byte[] { Info[2], Info[3], Info[4], Info[5] };
                            byte[] SL = new byte[] { Info[6], Info[7], Info[8], Info[9] };
                            byte[] PNA = new byte[] { Info[10 + NILength], Info[11 + NILength] };
                            byte DeviceType = Info[12 + NILength];
                            byte Status = Info[13 + NILength];
                            byte[] ProfileID = new byte[] { Info[14 + NILength], Info[15 + NILength] };
                            byte[] ManufacturerID = new byte[] { Info[16 + NILength], Info[17 + NILength] };
                            byte[] NIByte = new byte[NILength - 1];
                            for (int i = 0; i < NILength - 1; i++)
                            {
                                NIByte[i] = Info[i + 10];
                            }
                            String NI = System.Text.Encoding.ASCII.GetString(NIByte);

                            Modulus = new XBeeModule(SH, SL, MY, PNA, DeviceType, Status, ProfileID, ManufacturerID, NI);

                            foreach (XBeeModule Module in Modules)
                            {
                                if (Module.CompareAddress(SH, SL))
                                {
                                    found = true;
                                    Module.Update(MY, PNA, DeviceType, Status, ProfileID, ManufacturerID, NI);
                                }
                            }
                            if (!found)
                            {
                                Modules.Add(Modulus);
                            }
                        }
                        ATResponseFrames.Enqueue(T);
                        break;
                    case 0x97:
                        ATRemoteResponse Y  = new ATRemoteResponse(R);
                        if(Y.getATCommand() == "ND")
                        {
                            byte[] Info = Y.getDataBytes();
                            //Since this is an ND response we will create a module if needed and update the existing item if not needed
                            /*
                            MY (2 Bytes)
                            SH (4 Bytes)
                            SL (4 Bytes)
                            NI (Variable length)
                            PARENT_NETWORK ADDRESS (2 Bytes)
                            DEVICE_TYPE (1 Byte: 0 = Coord, 1 = Router, 2 = End Device)
                            STATUS(1 Byte: Reserved)
                            PROFILE_ID(2 Bytes)
                            MANUFACTURER_ID(2 Bytes)
                            */
                            int NILength = Info.Length - 18;
                            byte[] MY = new byte[] { Info[0], Info[1] };
                            byte[] SH = new byte[] { Info[2], Info[3], Info[4], Info[5] };
                            byte[] SL = new byte[] { Info[6], Info[7], Info[8], Info[9] };
                            byte[] PNA = new byte[] { Info[10 + NILength], Info[11 + NILength] };
                            byte DeviceType = Info[12 + NILength];
                            byte Status = Info[13 + NILength];
                            byte[] ProfileID = new byte[] { Info[14 + NILength], Info[15 + NILength] };
                            byte[] ManufacturerID = new byte[] { Info[16 + NILength], Info[17 + NILength] };
                            byte[] NIByte = new byte[NILength - 1];
                            for (int i = 0; i < NILength - 1; i++)
                            {
                                NIByte[i] = Info[i + 10];
                            }
                            String NI = System.Text.Encoding.ASCII.GetString(NIByte);
                            if (Me.CompareAddress(SH, SL))
                            {
                                Me.Update(ParentNetworkAddress: PNA, Device: DeviceType, Stats: Status, Profile_ID: ProfileID, Manufacturer_ID: ManufacturerID);
                            }
                        }

                        bool added = false;
                        foreach (XBeeModule X in Modules)
                        {
                            if (!added)
                            {
                                if (X.myPacketResponse(0x17, Y.getFrameID(), 0x97)) { X.addATRemoteResponse(Y); added = true; }
                            }
                        }
                        if (!added) { UnknownATRemoteResponseFrames.Enqueue(Y); }
                        break;

                    case 0x8A:
                        ModemStatusFrames.Enqueue(new ModemStatus(R));
                        break;
                    case 0x99:
                        ReceivePacket Rp = new ReceivePacket(R);
                        bool Rpadded = false;
                        foreach (XBeeModule X in Modules)
                        {
                            if (!Rpadded)
                            {
                                bool match = true;
                                List<Byte> XAD64 = X.get64bitAddress();
                                List<Byte> AD64 = Rp.getAddress64();
                                for (int i = 0; i < 8; i++)
                                {
                                    if (XAD64[i] != AD64[i]) { match = false; }
                                }
                                if (match) { X.addReceivePacket(Rp); Rpadded = true; }
                            }
                        }
                        if (!Rpadded) { UnknownReceivePacketFrames.Enqueue(Rp); }
                        break;
                    case 0x8B:
                        TransmitStatus Ts = new TransmitStatus(R);
                        TransmitFrames.Enqueue(Ts);
                        break;
                    default:
                        GenericFrames.Enqueue(R);
                        break;
                }

                if (!initialOutSideView && Modules.Count > 0)
                {
                    ATRemoteCommand RND = new ATRemoteCommand("ND", Modules[0], 1, 2, Me.getName());
                    sendPacket(RND);
                    initialOutSideView = true;
                }
                OnPacketReceived(EventArgs.Empty);
            }
        }
        public TransmitStatus getTransmitPacket()
        {
            if (TransmitFrames.Count > 0)
            {
                return TransmitFrames.Dequeue();
            }
            return null;
        }
        public ATResponse getATResponsePacket()
        {
            if (ATResponseFrames.Count > 0)
            {
                return ATResponseFrames.Dequeue();
            }
            return null;
        }
        public ModemStatus getModemStatusPacket()
        {
            if (ModemStatusFrames.Count > 0)
            {
                return ModemStatusFrames.Dequeue();
            }
            return null;
        }
        public int getAllModuleandLocalFrameCounts()
        {
            int Count = 0;
            foreach(XBeeModule X in Modules)
            {
                Count = Count + X.getFramceCount();
            }
            Count = Count + ModemStatusFrames.Count;
            Count = Count + ATResponseFrames.Count;
            Count = Count + TransmitFrames.Count;
            Count = Count + UnknownATRemoteResponseFrames.Count;
            Count = Count + UnknownReceivePacketFrames.Count;
            return Count;
        }
        public void sendPacket(XBeeAPIFrame Packet)
        {
            XBSerial.SendPacket(Packet);
        }
        public XBeeModule getRemoteXBee(string Name)
        {
            XBeeModule M = null;
            foreach (XBeeModule Module in Modules)
            {
                if(Module.getName() == Name)
                {
                    M = Module;
                }
            }
            return M;
        }
        public List<string> getModuleNames()
        {
            List<string> N = new List<string>();
            foreach(XBeeModule Module in Modules)
            {
                N.Add(Module.getName());
            }
            return N;
        }
        public int getTransmitStatusFrameCounts()
        {
            return TransmitFrames.Count;
        }
        public int getATResponseFrameCount()
        {
            return ATResponseFrames.Count;
        }
        public int getModemStatusFrameCount()
        {
            return ModemStatusFrames.Count;
        }
        public int getGenericFrameCount()
        {
            return GenericFrames.Count();
        }
        public int getModulesFrameCount()
        {
            int Count = 0;
            foreach (XBeeModule X in Modules)
            {
                Count = Count + X.getFramceCount();
            }
            return Count;
        }
        public List<XBeeModule> getModules()
        {
            return Modules;
        }
    }
}
