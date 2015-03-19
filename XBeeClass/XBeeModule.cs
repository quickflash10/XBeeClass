using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBeeClass
{
    public class XBeePacketLink
    {
        public byte TypeIn;
        public byte FrameID;
        public byte TypeOut;
        public DateTime Created;
        public XBeePacketLink(byte Type_In, byte Type_Out, byte Frame_ID)
        {
            TypeIn = Type_In;
            TypeOut = Type_Out;
            FrameID = Frame_ID;
            Created = DateTime.Now;
        }

    }
    public class XBeeModule
    {
        private List<byte> Address64 = new List<byte>();
        private List<byte> Address16 = new List<byte>();
        private List<byte> PNA = new List<byte>();
        private string DeviceType;
        private byte Status;
        private List<byte> ProfileID = new List<byte>();
        private List<byte> ManufacturerID = new List<byte>();
        private string Name;
        private List<byte> _SH = new List<byte>();
        private List<byte> _SL = new List<byte>();
        private List<XBeePacketLink> PacketLinks = new List<XBeePacketLink>();

        Queue<ATRemoteResponse> ATRemoteResponseFrames = new Queue<ATRemoteResponse>();
        Queue<ReceivePacket> ReceivePacketFrames = new Queue<ReceivePacket>();
        private Dictionary<DateTime, byte> RSSI = new Dictionary<DateTime, byte>();
        private Dictionary<DateTime, byte> PacketType = new Dictionary<DateTime, byte>();

        public XBeeModule(byte[] SH, byte[] SL, byte[] MY, byte[] ParentNetworkAddress, byte Device, byte Stats, byte[] Profile_ID, byte[] Manufacturer_ID, string NI)
        {
            Address64.AddRange(SH);
            Address64.AddRange(SL);
            Address16.AddRange(MY);
            PNA.AddRange(ParentNetworkAddress);
            switch (Device)
            {
                case 0:
                    DeviceType = "Coordinater";
                    break;
                case 1:
                    DeviceType = "Router";
                    break;
                case 2:
                    DeviceType = "End Point";
                    break;
                default:
                    DeviceType = "End Point";
                    break;
            }
            Status = Stats;
            ProfileID.AddRange(ProfileID);
            ManufacturerID.AddRange(Manufacturer_ID);
            Name = NI;
        }
        public XBeeModule(List<byte> b64Address)
        {
            Address64 = b64Address;
        }
        public XBeeModule()
        {

        }
        public void Update(byte[] MY = null, byte[] ParentNetworkAddress = null, byte Device = 5, byte Stats = 0, byte[] Profile_ID = null, byte[] Manufacturer_ID = null, string NI = null)
        {
            if (MY != null)
            {
                Address16.Clear();
                Address16.AddRange(MY);
            }
            if (ParentNetworkAddress != null)
            {
                PNA.Clear();
                PNA.AddRange(ParentNetworkAddress);
            }
            if (Device != 5)
            {
                switch (Device)
                {
                    case 0:
                        DeviceType = "Coordinater";
                        break;
                    case 1:
                        DeviceType = "Router";
                        break;
                    case 2:
                        DeviceType = "End Point";
                        break;
                    default:
                        DeviceType = "End Point";
                        break;
                }
            }
            if (Stats != 0)
            {
                Status = Stats;
            }
            if (Profile_ID != null)
            {
                ProfileID.Clear();
                ProfileID.AddRange(Profile_ID);
            }
            if (Manufacturer_ID != null)
            {
                ManufacturerID.Clear();
                ManufacturerID.AddRange(Manufacturer_ID);
            }
            if (NI != null)
            {
                Name = NI;
            }
        }
        public XBeeModule(List<byte> b64Address, List<byte> b16Address)
        {
            Address16 = b16Address;
            Address64 = b64Address;
        }
        public void SetAddress64(byte[] SH = null, byte[] SL = null)
        {
            if(Address64.Count == 0)
            {
                if(SH != null)
                {
                    if(_SH.Count == 0)
                        _SH.AddRange(SH);
                   
                }
                if(SL != null)
                {
                    if (_SL.Count == 0)
                        _SL.AddRange(SL);
                }
                if(_SH.Count > 0 && _SL.Count > 0)
                {
                    Address64.AddRange(_SH);
                    Address64.AddRange(_SL);
                }
            }
        }
        public List<byte>  get64bitAddress()
        {
            return Address64;
        }
        public List<byte> get16bitAddress()
        {
            return Address16;
        }
        public string getDeviceType()
        {
            return DeviceType;
        }
        public bool CompareAddress(byte[] SH, byte[] SL)
        {
            List<byte> CompareAddress64 = new List<byte>();
            bool Same = true;
            CompareAddress64.AddRange(SH);
            CompareAddress64.AddRange(SL);
            for (int i = 0; i < Address64.Count; i++)
            {
                if(Address64[i] != CompareAddress64[i])
                {
                    Same = false;
                }
            }
            return Same;
        }
        public string getName()
        {
            return Name;
        }
        public ReceivePacket getNextReceivePacket()
        {
            if(ReceivePacketFrames.Count > 0)
            {
                return ReceivePacketFrames.Dequeue();
            }
            else
            {
                return null;
            }

        }
        public ATRemoteResponse getNextATRemoteResponse()
        {
            if (ATRemoteResponseFrames.Count > 0)
            {
                return ATRemoteResponseFrames.Dequeue();
            }
            else
            {
                return null;
            }

        }
        public int getATRemoteResponseFrameCount() { return ATRemoteResponseFrames.Count; }
        public int getReceivePacketFrameCount() { return ReceivePacketFrames.Count; }
        public int getFramceCount() { return ReceivePacketFrames.Count + ATRemoteResponseFrames.Count; }
        public void addATRemoteResponse(ATRemoteResponse Frame)
        {
            ATRemoteResponseFrames.Enqueue(Frame);
        }
        public void addReceivePacket(ReceivePacket Frame)
        {
            ReceivePacketFrames.Enqueue(Frame);
        }
        public void addPacketLink(byte Type_In, byte Frame_ID, byte Type_Out)
        {
            PacketLinks.Add(new XBeePacketLink(Type_In, Type_Out, Frame_ID));
        }
        public bool myPacketResponse(byte Type_In, byte Frame_ID, byte Type_Out)
        {
            bool Mine = false;
            List<int> RemoveAt = new List<int>();
            for(int i = 0; i < PacketLinks.Count; i++)
            {
                if(((DateTime.Now) - PacketLinks[i].Created).TotalSeconds > 60)
                {
                    RemoveAt.Add(i);
                }
                else
                {
                    if(Mine == false && Type_In == PacketLinks[i].TypeIn && PacketLinks[i].FrameID == Frame_ID && PacketLinks[i].TypeOut == Type_Out)
                    {
                        Mine = true;
                        RemoveAt.Add(i);
                    }
                }
            }

            RemoveAt.Sort((s1, s2) => s1.CompareTo(s2));
            RemoveAt.Reverse();

            foreach(int R in RemoveAt)
            {
                PacketLinks.RemoveAt(R);
            }
            return Mine;
        }
    }
}
